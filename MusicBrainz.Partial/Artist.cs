namespace MusicBrainz.Partial;

public class Artist : Entity {
  public string Name { get; init; }
  public ICollection<ReleaseArtistCredit> ReleaseArtistCredits { get; set; }
  public ICollection<Release> Releases { get; set; }
}