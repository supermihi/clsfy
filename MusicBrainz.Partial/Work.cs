namespace MusicBrainz.Partial; 

public class Work : Entity, ISimpleWork {
  public string Title { get; set; }
  public ICollection<Recording> Recordings { get; set; }
  public ICollection<Work> Parts { get; set; } = new List<Work>();
  public ICollection<WorkWorkRelation> PartRelations { get; set; } = new List<WorkWorkRelation>();
  public ICollection<Work> Containing { get; set; } = new List<Work>();
  public ICollection<WorkWorkRelation> ContainingRelations { get; set; } = new List<WorkWorkRelation>();
}

public class WorkWorkRelation {
  public Guid ContainingId { get; set; }
  public Guid PartId { get; set; }
  
  public Work Containing { get; set; }
  public Work Part { get; set; }
  public ICollection<Guid> ComposerIds { get; set; }
  public ICollection<Artist> Composers { get; set; }
  
}
