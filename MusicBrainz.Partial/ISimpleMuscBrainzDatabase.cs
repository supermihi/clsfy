namespace MusicBrainz.Partial;

public interface ISimpleMuscBrainzDatabase {
  IAsyncEnumerable<IRelease> GetReleasesAsync(CancellationToken cancellationToken = default);
}