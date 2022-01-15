namespace MusicBrainz.Partial;

public class Artist : Entity, ISimpleArtist {
  public string Name { get; init; }
  public ICollection<ReleaseArtistCredit> ReleaseArtistCredits { get; set; }
  public ICollection<Release> Releases { get; set; }
}