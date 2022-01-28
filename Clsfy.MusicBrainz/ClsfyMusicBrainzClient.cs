using Clsfy.MusicBrainz.Interface;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Microsoft.Extensions.Logging;

namespace Clsfy.MusicBrainz;

public class ClsfyMusicBrainzClient : IClsfyMusicBrainzClient {
  private readonly Query _query;
  private readonly RecordingRelationFactory _relationFactory;
  private readonly ILogger<ClsfyMusicBrainzClient>? _logger;

  public ClsfyMusicBrainzClient(IQueryWrapper query, RecordingRelationFactory relationFactory,
      ILogger<ClsfyMusicBrainzClient>? logger = null) {
    _query = query.Query;
    _relationFactory = relationFactory;
    _logger = logger;
  }

  public async Task<Release> GetReleaseAsync(Guid id) {
    var mbRelease =
        await _query.LookupReleaseAsync(id,
                                        Include.Media         |
                                        Include.Recordings    |
                                        Include.ArtistCredits |
                                        Include.Artists       |
                                        Include.ReleaseRelationships);
    return mbRelease.ToRelease();
  }

  public async Task<Recording> GetRecordingAsync(Guid id) {
    var mbRecording = await _query.LookupRecordingAsync(id, Include.WorkRelationships | Include.ArtistRelationships | Include.Aliases);

    var relations = mbRecording.Relationships!.Select(_relationFactory.Create).ToList();
    var works = relations.Where(r => r.Work           != null).Select(r => r.Work!.WorkId).ToList();
    var performers = relations.Where(r => r.Performer != null).Select(r => r.Performer!).ToList();
    var title = mbRecording.GetPreferredAlias() ?? mbRecording.Title ??
        throw new MusicBrainzContractException("recording", id, nameof(IRecording.Title), "title is null");
    return new Recording(id, title, works, performers);
  }

  public async Task<Work> GetWorkAsync(Guid id) {
    var mbWork =
        await _query.LookupWorkAsync(id, Include.WorkRelationships | Include.ArtistRelationships | Include.Aliases);
    var title = mbWork.GetPreferredAlias() ?? mbWork.Title ?? throw new ArgumentException("missing work title");
    var partOf = new List<Guid>();
    var composers = new List<Guid>();
    foreach (var relation in mbWork.Relationships!) {
      switch (relation.Type!, relation.Direction!) {
        case ("parts", "backward"):
          partOf.Add(relation.Work!.Id);
          break;
        case ("composer", "backward"):
          composers.Add(relation.Artist!.Id);
          break;
        // ignored relations
        case ("lyricist", _):
        // we're not interested in parts of the work, unless we have recordings of them –
        // in which case they will be added with those
        case ("parts", "forward"):
        // no need to handle arrangement of this work
        case ("arrangement", "forward"):
        // works based on this one
        case ("based on", "forward"):
        // referred to in medley
        case ("medley", "backward"):
        case ("other version", _):
        case ("dedication", _):
          _logger?.LogDebug("Ignoring work-{TargetType} relationship {Type} of work {WorkId}", relation.TargetType,
                            relation.Type, mbWork.Id);
          break;
        default:
          throw new ArgumentException($"work relation {relation} not implemented");
      }
    }
    return new Work(id, title, partOf, composers);
  }

  public async Task<Artist> GetArtistAsync(Guid id) {
    var mbArtist = await _query.LookupArtistAsync(id, Include.Aliases);
    var name = mbArtist.GetPreferredAlias() ?? mbArtist.Name ?? throw new MusicBrainzContractException("artist",
      id, nameof(mbArtist.Name), "missing artist name");
    return new Artist(mbArtist.Id, name);
  }

  public async Task<Instrument> GetInstrumentAsync(Guid id) {
    var mbInstrument = await _query.LookupInstrumentAsync(id, Include.Aliases);
    var name = mbInstrument.GetPreferredAlias() ?? mbInstrument.Name ?? throw new MusicBrainzContractException(
      "instrument", id, nameof(mbInstrument.Name),
      "missing instrument name");
    return new Instrument(id, name);
  }
}
