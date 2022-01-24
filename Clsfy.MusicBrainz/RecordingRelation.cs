using System.Diagnostics;
using Clsfy.Model.Common;

namespace Clsfy.MusicBrainz;

public record RecordingRelation {
  public Interface.Performer? Performer { get; }
  public RecordedWork? Work { get; }

  private RecordingRelation(Interface.Performer? performer, RecordedWork? work) {
    Performer = performer;
    Work = work;
    Debug.Assert((performer == null) ^ (work == null));
  }

  public record RecordingPerformer(PerformanceType Type, string? Instrument = null);

  public static RecordingRelation Performance(Guid artist, PerformanceType type, string? instrument = null) =>
      new(new(artist, type, instrument), null);

  public record RecordedWork(Guid WorkId);
}
