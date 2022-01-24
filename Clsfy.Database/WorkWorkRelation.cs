using System.Diagnostics.CodeAnalysis;

namespace Clsfy.Database;

public class WorkWorkRelation {
  public Guid ContainingId { get; set; }
  public Guid PartId { get; set; }

  public Work Containing { get; set; } = null!;
  public Work Part { get; set; } = null!;
}