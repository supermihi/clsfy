namespace Clsfy.MusicBrainz.Interface;

public interface IClsfyMusicBrainzClient {
  Task<Release> GetReleaseAsync(Guid id);
  Task<Recording> GetRecordingAsync(Guid id);
  Task<Work> GetWorkAsync(Guid id);
}
