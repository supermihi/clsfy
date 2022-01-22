namespace MusicBrainz.Partial;

public class WorkArtistRelation {
  public Guid WorkId { get; set; }
  public Work Work { get; set; } = null!;
  public Guid ArtistId { get; set; } 
  public Artist Artist { get; set; } = null!;
}
