using System.Diagnostics.CodeAnalysis;

namespace MusicBrainz.Partial;

public class WorkWorkRelation {
  public Guid ContainingId { get; set; }
  public Guid PartId { get; set; }

  public Work Containing { get; set; } = null!;
  public Work Part { get; set; } = null!;
}