using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MusicBrainz.Partial;

public class PartialMusicBrainzDatabase : ISimpleMuscBrainzDatabase {
  private readonly MusicBrainzContext _db;
  private readonly Query _musicBrainzClient;
  private readonly ILogger _logger;

  public PartialMusicBrainzDatabase(MusicBrainzContext dbContext, Query musicBrainzClient, ILogger? logger = null) {
    _db = dbContext;
    _musicBrainzClient = musicBrainzClient;
    _logger = logger ?? NullLogger.Instance;
  }

  public async Task<Release> AddRelease(Guid mbid) {
    var release = await _db.Releases.FirstOrDefaultAsync(r => r.Id.Equals(mbid));
    if (release != null) {
      _logger.LogInformation("found release");
      return release;
    }
    var theRelease =
        await _musicBrainzClient.LookupReleaseAsync(mbid,
                                                    Include.Media | Include.Recordings | Include.DiscIds |
                                                    Include.ArtistCredits | Include.Artists);
    return await AddRelease(theRelease);
  }

  private async Task<Release> AddRelease(MetaBrainz.MusicBrainz.Interfaces.Entities.IRelease mbRelease) {
    var release = new Release { Id = mbRelease.Id, Title = mbRelease.Title ?? "", Date = mbRelease.Date?.NearestDate };
    foreach (var mbMedium in mbRelease.Media!.Take(1)) {
      var medium = await CreateMedium(mbMedium);
      release.Media.Add(medium);
    }
    foreach (var mbArtistCredit in mbRelease.ArtistCredit!) {
      var artist = await GetOrCreateArtist(mbArtistCredit.Artist!);
      release.Artists.Add(artist);
    }
    await _db.AddAsync(release);
    await _db.SaveChangesAsync();
    return release;
  }

  private async Task<Medium> CreateMedium(IMedium mbMedium) {
    var medium = new Medium { Position = mbMedium.Position };
    foreach (var mbTrack in mbMedium.Tracks!.Take(1)) {
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
    var recording = await _db.Recordings.SingleOrDefaultAsync(recording => recording.Id == mbRecording.Id);
    if (recording != null) {
      return recording;
    }
    var result = new Recording
        { Id = mbRecording.Id, Title = mbRecording.Title ?? throw new ArgumentException("missing recording title") };
    return result;
  }

  private async Task<Artist> GetOrCreateArtist(IArtist mbArtist) {
    var artist = await _db.Artists.SingleOrDefaultAsync(a => a.Id == mbArtist.Id);
    if (artist != null) {
      return artist;
    }
    var result = new Artist
        { Id = mbArtist.Id, Name = mbArtist.Name ?? throw new ArgumentException("nameless artist") };
    await _db.AddAsync(result);
    return result;
  }

  public IAsyncEnumerable<IRelease> GetReleasesAsync(CancellationToken cancellationToken = default) {
    return _db.Releases.AsAsyncEnumerable();
  }
}