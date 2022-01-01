using MetaBrainz.MusicBrainz;

namespace MusicBrainz.Partial;

public class Release : Entity, IRelease {
  public string Title { get; set; }
  
  public ICollection<ReleaseArtistCredit> ReleaseArtistCredits { get; set; }
  public ICollection<Artist> Artists { get; set; } = new List<Artist>();
  
  public DateTime? Date { get; set; }
  public List<Medium> Media { get; set; } = new();


}