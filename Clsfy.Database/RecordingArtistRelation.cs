using Clsfy.Model.Common;

namespace Clsfy.Database;

public class RecordingArtistRelation {
  public Recording Recording { get; set; } = null!;
  public Guid RecordingId { get; set; }
  public Artist Artist { get; set; } = null!;
  public Guid ArtistId { get; set; }
  public PerformanceType Type { get; set; }
  public Instrument? Instrument { get; set; }
}
