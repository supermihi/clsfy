namespace MusicBrainz.Partial;

public class Recording : Entity {
  public string Title { get; set; }
  public ICollection<Work> Works { get; set; } = new List<Work>();
}