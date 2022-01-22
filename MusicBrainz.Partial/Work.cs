namespace MusicBrainz.Partial; 

public class Work : Entity, ISimpleWork {
  public string Title { get; set; } = null!;
  public ICollection<Recording> Recordings { get; set; } = null!;
  public ICollection<Work> Parts { get; set; } = new List<Work>();
  public ICollection<WorkWorkRelation> PartRelations { get; set; } = new List<WorkWorkRelation>(); 
  public ICollection<Work> Containing { get; set; } = new List<Work>();
  public ICollection<WorkWorkRelation> ContainingRelations { get; set; } = new List<WorkWorkRelation>();
  public ICollection<WorkArtistRelation> ArtistRelations { get; set; } = new List<WorkArtistRelation>();
  public ICollection<Artist> Artists { get; set; } = new List<Artist>();
}