namespace Clsfy.Database;

public class Recording : Entity {
  public string Title { get; set; } = null!;
  public ICollection<Work> Works { get; set; } = new List<Work>();
  public ICollection<RecordingArtistRelation> PerformerRelations { get; set; } = new List<RecordingArtistRelation>();
  public ICollection<Artist> Performers { get; set; } = new List<Artist>();
}