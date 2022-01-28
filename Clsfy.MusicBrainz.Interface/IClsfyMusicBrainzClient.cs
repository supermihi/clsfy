namespace Clsfy.MusicBrainz.Interface;

public interface IClsfyMusicBrainzClient {
  Task<Release> GetReleaseAsync(Guid id);
  Task<Recording> GetRecordingAsync(Guid id);
  Task<Work> GetWorkAsync(Guid id);
  Task<Artist> GetArtistAsync(Guid id);
  Task<Instrument> GetInstrumentAsync(Guid id);
}
