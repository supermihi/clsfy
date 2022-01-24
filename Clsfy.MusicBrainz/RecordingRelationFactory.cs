using Clsfy.Model.Common;
using Clsfy.MusicBrainz.Interface;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace Clsfy.MusicBrainz;

public class RecordingRelationFactory {
  private readonly Query _query;
  public static readonly Guid Instrument = new("59054b12-01ac-43ee-a618-285fd397e461");
  public static readonly Guid Orchestra = new("3b6616c5-88ba-4341-b4ee-81ce1e6d7ebb");
  public static readonly Guid Conductor = new("234670ce-5f22-4fd0-921b-ef1662695c5d");

  // work-recording
  public static readonly Guid Performance = new("a3005666-a872-32c3-ad06-98af558e99b0");
  // work-work
  public static readonly Guid Part = new("ca8d3642-ce5f-49f8-91f2-125d72524e6a");
  private readonly Dictionary<Guid, Func<IRelationship, RecordingRelation>> _factories;

  static RecordingRelationFactory Default(Query query) {
    var result = new RecordingRelationFactory(query);
    result.Register(Orchestra, r => RecordingRelation.Performance(r.Artist!.Id, PerformanceType.Orchestra));
    result.Register(Conductor, r => RecordingRelation.Performance(r.Artist!.Id, PerformanceType.Conductor));
    result.Register(
        Instrument, r => RecordingRelation.Performance(r.Artist!.Id, PerformanceType.Instrument, r.Attributes!.Single() /* TODO */));
    return result;
  }

  public RecordingRelationFactory(Query query) {
    _query = query;
    _factories = new Dictionary<Guid, Func<IRelationship, RecordingRelation>>();
  }

  public void Register(Guid typeId, Func<IRelationship, RecordingRelation> factory) {
    _factories[typeId] = factory;
  }

  public RecordingRelation Create(IRelationship relationship) {
    if (relationship.TypeId is null) {
      throw new MusicBrainzContractException("relationship", null, nameof(IRelationship.TypeId), "type ID is null");
    }
    if (_factories.TryGetValue(relationship.TypeId.Value, out var factory))
    {
      return factory(relationship);
    }
    throw new NotImplementedException($"relationship {relationship} not implemented");
  }
}
