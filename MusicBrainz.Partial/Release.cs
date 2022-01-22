using MetaBrainz.MusicBrainz;

namespace MusicBrainz.Partial;

public class Release : Entity, ISimpleRelease {
  public string Title { get; set; } = null!;

  public ICollection<ReleaseArtistCredit> ReleaseArtistCredits { get; set; } = null!;
  public ICollection<Artist> Artists { get; set; } = new List<Artist>();
  
  public DateTime? Date { get; set; }
  public List<Medium> Media { get; set; } = new();


}