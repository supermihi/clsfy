using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MusicBrainz.Partial;

public class PartialMusicBrainzDatabase : IAsyncDisposable {
  public MusicBrainzContext Db { get; }
  private readonly Query _musicBrainzClient;
  private readonly ILogger _logger;

  public PartialMusicBrainzDatabase(MusicBrainzContext dbContext, Query musicBrainzClient, ILogger? logger = null) {
    Db = dbContext;
    _musicBrainzClient = musicBrainzClient;
    _logger = logger ?? NullLogger.Instance;
  }

  public async Task<Release> AddRelease(Guid mbid) {
    var release = await Db.Releases.FindAsync(mbid);
    if (release != null) {
      _logger.LogInformation("Release {Release} already exists", release.Title);
      return release;
    }
    var mbRelease =
        await _musicBrainzClient.LookupReleaseAsync(mbid,
                                                    Include.Media | Include.Recordings | Include.DiscIds |
                                                    Include.ArtistCredits | Include.Artists | Include.WorkRelationships
                                                    | Include.ArtistRelationships
                                                    | Include.RecordingRelationships | Include.WorkLevelRelationships
                                                    | Include.ReleaseRelationships | Include.RecordingLevelRelationships);
    _logger.LogInformation("Adding new release {Release} with {NumMedia} medial", mbRelease.Title, mbRelease.Media!.Count);
    return await AddRelease(mbRelease);
  }

  private async Task<Release> AddRelease(IRelease mbRelease) {
    var release = new Release { Id = mbRelease.Id, Title = mbRelease.Title ?? "", Date = mbRelease.Date?.NearestDate };
    foreach (var mbMedium in mbRelease.Media!) {
      var medium = await CreateMedium(mbMedium);
      release.Media.Add(medium);
    }
    foreach (var mbArtistCredit in mbRelease.ArtistCredit!) {
      var artist = await GetOrCreateArtist(mbArtistCredit.Artist!);
      release.Artists.Add(artist);
    }
    await Db.AddAsync(release);
    await Db.SaveChangesAsync();
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
    recording = new Recording { Id = mbRecording.Id, Title = mbRecording.Title ?? throw new ArgumentException("missing recording title") };
    var rels = mbRecording.Relationships!.Where(t => t.TargetType == EntityType.Work);
    foreach (var workRelation in rels) {
      if (workRelation.Type != "performance") {
        _logger.LogWarning("ignoring unsupported recording→work relation {Type} on {Recording} with id {Id}", workRelation.Type, mbRecording.Title, mbRecording.Id);
        continue;
      }
      _logger.LogInformation("Found work {Work} performed on {Recording} with id {Id}", workRelation.Work!.Title, mbRecording.Title, mbRecording.Id);
      var work = await GetOrCreateWork(workRelation.Work!);
      recording.Works.Add(work);
    }
    return recording;
  }

  private async Task<Work> GetOrCreateWork(IWork mbWork) {
    var existing = await Db.Works.FindAsync(mbWork.Id);
    if (existing != null) {
      return existing;
    }
    var work = new Work { Id = mbWork.Id, Title = mbWork.Title ?? throw new ArgumentException("missing work title") };
    Db.Works.Add(work);
    foreach (var relation in mbWork.Relationships ?? Enumerable.Empty<IRelationship>()) {
      switch (relation.Type!, relation.Direction!) {
        case ("parts", "forward"):
          var part = await GetOrCreateWork((IWork)relation.Target!);
          work.Parts.Add(part);
          break;
        case ("parts", "backward"):
          var containing = await GetOrCreateWork((IWork)relation.Target!);
          work.Containing.Add(containing);
          break;
        case ("backward", "composer"):
          var composer = await GetOrCreateArtist((IArtist)relation.Target!);
          work.Composers.Add(composer);
        default:
          throw new ArgumentException($"work relation {relation} not implemented");
      }
    }
    return work;
  }

  private async Task<Artist> GetOrCreateArtist(IArtist mbArtist) {
    var artist = await Db.Artists.FindAsync(mbArtist.Id);
    if (artist != null) {
      return artist;
    }
    var result = new Artist
        { Id = mbArtist.Id, Name = mbArtist.Name ?? throw new ArgumentException("nameless artist") };
    await Db.AddAsync(result);
    return result;
  }
  public ValueTask DisposeAsync() {
    _musicBrainzClient.Dispose();
    return ValueTask.CompletedTask;
  }
}