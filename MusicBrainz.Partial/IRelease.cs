namespace MusicBrainz.Partial; 

public interface ISimpleRelease {
  string Title { get; }
  DateTime? Date { get; }
}