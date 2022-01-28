namespace Clsfy.Database;

public class Artist : Entity, ISimpleArtist {
  public string Name { get; init; } = null!;
  public ICollection<ReleaseArtistCredit> ReleaseArtistCredits { get; set; } = null!;
  public ICollection<Release> Releases { get; set; } = null!;

  public ICollection<WorkArtistRelation> WorkRelations { get; set; } = null!;
  public ICollection<Work> RelatedWorks { get; set; } = null!;

  public ICollection<Recording> Recordings { get; set; } = null!;
  public ICollection<RecordingArtistRelation> RecordingRelations { get; set; } = null!;
}
