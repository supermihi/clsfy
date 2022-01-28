using Clsfy.MusicBrainz.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ClsfyMB = Clsfy.MusicBrainz.Interface;

namespace Clsfy.Database;

public class PartialMusicBrainzDatabase {
  public MusicBrainzContext Db { get; }
  private readonly IClsfyMusicBrainzClient _client;
  private readonly ILogger _logger;

  public PartialMusicBrainzDatabase(MusicBrainzContext dbContext, IClsfyMusicBrainzClient client,
      ILogger? logger = null) {
    Db = dbContext;
    Db.Database.EnsureCreated();
    _client = client;
    _logger = logger ?? NullLogger.Instance;
  }

  public async Task<Release> GetOrAddRelease(Guid mbid) {
    var release = await Db.Releases.FindAsync(mbid);
    if (release != null) {
      _logger.LogInformation("Release {Release} already exists", release.Title);
      return release;
    }
    return await AddRelease(mbid);
  }

  private async Task<Release> AddRelease(Guid id) {
    var mbRelease = await _client.GetReleaseAsync(id);
    _logger?.LogInformation("Adding new release {Release} with {NumMedia} medial", mbRelease.Title,
                            mbRelease.Media.Count);
    var release = new Release { Id = mbRelease.Id, Title = mbRelease.Title, Date = mbRelease.Date };
    foreach (var mbMedium in mbRelease.Media) {
      var medium = await CreateMedium(mbMedium);
      release.Media.Add(medium);
    }
    foreach (var artistId in mbRelease.ArtistCredits) {
      var artist = await GetOrCreateArtist(artistId);
      release.Artists.Add(artist);
    }
    await Db.AddAsync(release);
    await Db.SaveChangesAsync();
    _logger?.LogInformation("Release {Release} added", mbRelease);
    return release;
  }

  private async Task<Medium> CreateMedium(ClsfyMB.Medium mbMedium) {
    var medium = new Medium { Position = mbMedium.Position };
    foreach (var mbTrack in mbMedium.Tracks) {
      var track = await CreateTrack(mbTrack);
      medium.Tracks.Add(track);
    }
    return medium;
  }

  private async Task<Track> CreateTrack(ClsfyMB.Track mbTrack) {
    var track = new Track {
        Id = mbTrack.Id, Length = mbTrack.Length,
        Position = mbTrack.Position,
        Recording = await GetOrCreateRecording(mbTrack.RecordingId)
    };
    return track;
  }

  private async Task<Recording> GetOrCreateRecording(Guid id) {
    var recording = await Db.Recordings.FindAsync(id);
    if (recording != null) {
      _logger.LogInformation("Recording with id {Id} already exists: {Title}", id, recording.Title);
      return recording;
    }
    var mbRecording = await _client.GetRecordingAsync(id);
    recording = new Recording
        { Id = id, Title = mbRecording.Title };
    foreach (var workId in mbRecording.RecordedWorkIds) {
      var work = await GetOrCreateWork(workId);
      recording.Works.Add(work);
      break;
    }
    foreach (var performer in mbRecording.Performers) {
      var artist = await GetOrCreateArtist(performer.ArtistId);
      var relation = new RecordingArtistRelation() {
          ArtistId = artist.Id, RecordingId = recording.Id, Type = performer.Type,
          Instrument = performer.Instrument != null ? await GetOrCreateInstrument(performer.Instrument!.Value) : null
      };
      recording.PerformerRelations.Add(relation);
    }
    return recording;
  }

  private async Task<Instrument> GetOrCreateInstrument(Guid id) {
    var existing = await Db.Instruments.FindAsync(id);
    if (existing != null) {
      return existing;
    }
    var (guid, name) = await _client.GetInstrumentAsync(id);
    var instrument = new Instrument { Id = guid, Name = name };
    await Db.AddAsync(instrument);
    return instrument;
  }

  private async Task<Work> GetOrCreateWork(Guid id) {
    var existing = await Db.Works.FindAsync(id);
    if (existing != null) {
      return existing;
    }
    var mbWork = await _client.GetWorkAsync(id);
    var work = new Work { Id = mbWork.Id, Title = mbWork.Title };
    foreach (var partOf in mbWork.PartOf) {
      var superWork = await GetOrCreateWork(partOf);
      work.ContainingRelations.Add(new WorkWorkRelation() { ContainingId = superWork.Id, PartId = work.Id});
    }
    foreach (var composerId in mbWork.Composers) {
      var composer = await GetOrCreateArtist(composerId);
      var rel = new WorkArtistRelation { Artist = composer, Work = work };
      work.ArtistRelations.Add(rel);
    }
    await Db.AddAsync(work);
    return work;
  }

  private async Task<Artist> GetOrCreateArtist(Guid id) {
    var artist = await Db.Artists.FindAsync(id);
    if (artist != null) {
      return artist;
    }
    var (guid, name) = await _client.GetArtistAsync(id);
    artist = new Artist { Id = guid, Name = name };
    await Db.AddAsync(artist);
    return artist;
  }
}
