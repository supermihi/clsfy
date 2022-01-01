namespace MusicBrainz.Partial; 

public interface IRelease {
  string Title { get; set; }
  DateTime? Date { get; set; }
}