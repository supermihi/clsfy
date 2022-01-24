using System.Diagnostics;
using Clsfy.MusicBrainz.Interface;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace Clsfy.MusicBrainz;

public static class Converters {
  public static Release ToRelease(this IRelease release) {
    if (release.Title is null) {
      throw new MusicBrainzContractException("release", release.Id, nameof(IRelease.Title), "title is null");
    }
    Debug.Assert(release.Media != null);
    Debug.Assert(release.ArtistCredit != null);
    return new Release(release.Id, release.Title, release.Date?.NearestDate, release.Media!.Select(ToMedium).ToList(),
                       release.ArtistCredit!.Select(a => a.Artist!.Id).ToList());
  }

  public static Medium ToMedium(this IMedium medium) {
    Debug.Assert(medium.Tracks != null);
    return new Medium(medium.Position, medium.Tracks!.Select(ToTrack).ToList());
  }

  public static Track ToTrack(this ITrack track) {
    if (track.Recording is null) {
      throw new MusicBrainzContractException("track", track.Id, nameof(track.Recording), "recording is null");
    }
    if (track.Position is null) {
      throw new MusicBrainzContractException("track", track.Id, nameof(track.Position), "position is null");
    }
    return new Track(track.Id, track.Position!.Value, track.Length, track.Recording!.Id);
  }

}
