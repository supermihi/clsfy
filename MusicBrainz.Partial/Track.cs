namespace MusicBrainz.Partial;

public class Track {
  public Guid Id { get; set; }
  public int MediumId { get; set; }
  public Medium Medium { get; set; } = null!;
  public int Position { get; set; }
  public TimeSpan? Length { get; set; }
  public Guid RecordingId { get; set; }
  public Recording Recording { get; set; } = null!;
}