namespace Clsfy.Database; 

public class Medium {
  public int Id { get; set; }
  public Guid ReleaseId { get; set; }
  public Release Release { get; set; } = null!;
  public int Position { get; set; }
  public List<Track> Tracks { get; } = new();
}