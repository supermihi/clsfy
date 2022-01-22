using MetaBrainz.MusicBrainz;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MusicBrainz.Partial;

public static class DatabaseFactory {
  public static PartialMusicBrainzDatabase Create(string databaseFile, string? musicBrainzHost,
                                                  ILoggerFactory? loggerFactory) {
    var context = new MusicBrainzContext(databaseFile);
    context.Database.EnsureCreated();

    var query = new Query("clsfy", "0.1.0", "michaelhelmling@posteo.de");
    if (musicBrainzHost != null) {
      var uri = new Uri(musicBrainzHost);
      query.Server = uri.Host;
      query.Port = uri.Port;
      query.UrlScheme = uri.Scheme;
      Query.DelayBetweenRequests = 0;
    }
    return new PartialMusicBrainzDatabase(context, query,
                                          loggerFactory?.CreateLogger<PartialMusicBrainzDatabase>() ??
                                          NullLogger<PartialMusicBrainzDatabase>.Instance);
  }
}