using MetaBrainz.MusicBrainz;
using Microsoft.Extensions.Logging;

namespace MusicBrainz.Partial; 

public static class DatabaseFactory {
  public static PartialMusicBrainzDatabase Create(string databaseFile) {
      var context = new MusicBrainzContext(databaseFile);
      context.Database.EnsureCreated();
      var query = new Query("clsfy", "0.1.0", "michaelhelmlnig@posteo.de");
      var logger = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)).CreateLogger<PartialMusicBrainzDatabase>();
      return new PartialMusicBrainzDatabase(context, query, logger);
  }
}