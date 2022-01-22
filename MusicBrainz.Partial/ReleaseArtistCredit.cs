namespace MusicBrainz.Partial; 

public class ReleaseArtistCredit {
  public Guid ReleaseId { get; set; }
  public Release Release { get; set; } = null!;
  public Guid ArtistId { get; set; }
  public Artist Artist { get; set; } = null!;
}