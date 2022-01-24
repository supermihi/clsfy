using MetaBrainz.MusicBrainz;

namespace Clsfy.MusicBrainz.Interface;

public class MusicBrainzContractException : Exception {
  public string EntityType { get; }
  public Guid? Mbid { get; }
  public string PropertyName { get; }
  public string Message { get; }

  public MusicBrainzContractException(string entityType, Guid? mbid, string propertyName, string message) {
    EntityType = entityType;
    Mbid = mbid;
    PropertyName = propertyName;
    Message = message;
  }

}
