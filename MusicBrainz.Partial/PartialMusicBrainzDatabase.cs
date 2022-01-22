using Clsfy.MusicBrainz;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MusicBrainz.Partial;

public class PartialMusicBrainzDatabase : IAsyncDisposable {
  public MusicBrainzContext Db { get; }
  private readonly Query _mb;
  private readonly ILogger _logger;

  public PartialMusicBrainzDatabase(MusicBrainzContext dbContext, Query mb, ILogger? logger = null) {
    Db = dbContext;
    _mb = mb;
    _logger = logger ?? NullLogger.Instance;
  }

  public async Task<Release> AddRelease(Guid mbid) {
    var release = await Db.Releases.FindAsync(mbid);
    if (release != null) {
      _logger.LogInformation("Release {Release} already exists", release.Title);
      return release;
    }
    var mbRelease =
        await _mb.LookupReleaseAsync(mbid,
                                     Include.Media | Include.Recordings | Include.DiscIds |
                                     Include.ArtistCredits | Include.Artists | Include.WorkRelationships
                                     | Include.ArtistRelationships | Include.InstrumentRelationships
                                     | Include.RecordingRelationships | Include.WorkLevelRelationships
                                     | Include.ReleaseRelationships | Include.RecordingLevelRelationships
                                     | Include.SeriesRelationships | Include.Aliases);
    _logger.LogInformation("Adding new release {Release} with {NumMedia} medial", mbRelease.Title,
                           mbRelease.Media!.Count);
    return await AddRelease(mbRelease);
  }

  private async Task<Release> AddRelease(IRelease mbRelease) {
    var release = new Release { Id = mbRelease.Id, Title = mbRelease.Title ?? "", Date = mbRelease.Date?.NearestDate };
    foreach (var mbMedium in mbRelease.Media!) {
      var medium = await CreateMedium(mbMedium);
      release.Media.Add(medium);
    }
    foreach (var mbArtistCredit in mbRelease.ArtistCredit!) {
      var artist = await GetOrCreateArtist(mbArtistCredit.Artist!.Id);
      release.Artists.Add(artist);
    }
    await Db.AddAsync(release);
    await Db.SaveChangesAsync();
    _logger.LogInformation("Release {Release} added", mbRelease);
    return release;
  }

  private async Task<Medium> CreateMedium(IMedium mbMedium) {
    var medium = new Medium { Position = mbMedium.Position };
    foreach (var mbTrack in mbMedium.Tracks!) {
      var track = await CreateTrack(mbTrack);
      medium.Tracks.Add(track);
    }
    return medium;
  }

  private async Task<Track> CreateTrack(ITrack mbTrack) {
    var mbRecording = mbTrack.Recording ?? throw new ArgumentException("missing recording in track");
    var track = new Track {
        Id = mbTrack.Id, Length = mbTrack.Length,
        Position = mbTrack.Position ?? throw new ArgumentException("missing position"),
        Recording = await GetOrCreateRecording(mbRecording)
    };
    return track;
  }

  private async Task<Recording> GetOrCreateRecording(IRecording mbRecording) {
    var recording = await Db.Recordings.FindAsync(mbRecording.Id);
    if (recording != null) {
      _logger.LogInformation("Recording with id {Id} already exists: {Title}", mbRecording.Id, recording.Title);
      return recording;
    }
    recording = new Recording
        { Id = mbRecording.Id, Title = mbRecording.Title ?? throw new ArgumentException("missing recording title") };
    foreach (var relation in mbRecording.Relationships!) {
      switch (relation.Type, relation.Direction) {
        case ("performance", "forward"):
          _logger.LogDebug("Found work {Work} performed on {Recording} with id {Id}", relation.Work!.Title,
                           mbRecording.Title, mbRecording.Id);
          var work = await GetOrCreateWork(relation.Work!.Id);
          recording.Works.Add(work);
          break;
        case ("conductor", "backward"):
          var conductor = await GetOrCreateArtist(relation.Artist!.Id);
          recording.PerformerRelations.Add(new RecordingArtistRelation()
                                       { Artist = conductor, Recording = recording, Type = PerformanceType.Conductor });
          break;
        case ("instrument", "backward"):
          var instrumentalist = await GetOrCreateArtist(relation.Artist!.Id);
          recording.PerformerRelations.Add(new RecordingArtistRelation() {
              Artist = instrumentalist, Recording = recording, Type = PerformanceType.Instrument,
              Instrument = relation.Attributes?.Any() ?? false ? Enum.Parse<Instrument>(relation.Attributes[0], true) : null
          });
          break;
        case ("performing orchestra", "backward"):
          var orchestra = await GetOrCreateArtist(relation.Artist!.Id);
          recording.PerformerRelations.Add(new RecordingArtistRelation() {
              Artist = orchestra, Recording = recording, Type = PerformanceType.Orchestra
          });
          break;
        default:
          _logger.LogWarning("ignoring unsupported recording relation {Relation} on {Recording}", relation,
                             mbRecording);
          break;
      }
    }
    return recording;
  }

  private async Task<Work> GetOrCreateWork(Guid id) {
    var existing = await Db.Works.FindAsync(id);
    if (existing != null) {
      return existing;
    }
    var mbWork = await _mb.LookupWorkAsync(id, Include.WorkRelationships | Include.ArtistRelationships | Include.Aliases);
    var work = new Work { Id = mbWork.Id, Title = mbWork.GetPreferredAlias() ?? mbWork.Title ?? throw new ArgumentException("missing work title") };
    await Db.AddAsync(work);
    foreach (var relation in mbWork.Relationships!) {
      switch (relation.Type!, relation.Direction!) {
        case ("parts", "backward"):
          var containing = await GetOrCreateWork(relation.Work!.Id);
          work.Containing.Add(containing);
          break;
        case ("composer", "backward"):
          var composer = await GetOrCreateArtist(relation.Artist!.Id);
          work.Artists.Add(composer);
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
          _logger.LogDebug("Ignoring work-{TargetType} relationship {Type} of work {WorkId}", relation.TargetType,
                           relation.Type, mbWork.Id);
          break;
        default:
          throw new ArgumentException($"work relation {relation} not implemented");
      }
    }
    return work;
  }

  private async Task<Artist> GetOrCreateArtist(Guid id) {
    var artist = await Db.Artists.FindAsync(id);
    if (artist != null) {
      return artist;
    }
    var mbArtist = await _mb.LookupArtistAsync(id, Include.Aliases);
    artist = new Artist { Id = mbArtist.Id, Name = mbArtist.GetPreferredAlias() ?? mbArtist.Name ?? throw new ArgumentException("unnamed artist") };
    await Db.AddAsync(artist);
    return artist;
  }

  public ValueTask DisposeAsync() {
    _mb.Dispose();
    return ValueTask.CompletedTask;
  }
}