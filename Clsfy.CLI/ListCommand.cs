using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicBrainz.Partial;

namespace Clsfy.CLI;

public class ListCommand : Command {
  private readonly IServiceProvider _services;

  public ListCommand(GlobalOptions options, IServiceProvider services) : base("list") {
    _services = services;
    var entityArgument = new Argument<EntityType>("entity", "the type of entity to list");
    AddArgument(entityArgument);
    this.SetHandler<string, string?, EntityType>(Handle, options.Database, options.Server, entityArgument);
  }

  private void Handle(string database, string? server, EntityType entityType) =>
      HandleAsync(database, server, entityType).GetAwaiter().GetResult();

  private async Task HandleAsync(string databasePath, string? server, EntityType entityType) {
    var database = DatabaseFactory.Create(databasePath, server, _services.GetService<ILoggerFactory>());
    switch (entityType) {
      case EntityType.Releases:
        await ListReleasesAsync(database);
        break;
      case EntityType.Artists:
        await ListArtistsAsync(database);
        break;
      case EntityType.Works:
        await ListWorksAsync(database);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
    }
  }

  private async Task ListWorksAsync(PartialMusicBrainzDatabase database) {
    var topLevelWorks = database.Db.Works.Where(work => !work.Containing.Any());
    await foreach (var work in topLevelWorks.Include(w => w.Artists).AsAsyncEnumerable()) {
      Console.WriteLine($"{work.Title} by {string.Join(',', work.Artists.Select(a => a.Name))}");
    }
  }

  private static async Task ListReleasesAsync(PartialMusicBrainzDatabase database) {
    await foreach (var release in database.Db.Releases.Include(r => r.Media).ThenInclude(m => m.Tracks)
                                          .ThenInclude(t => t.Recording).ThenInclude(t => t.Works)
                                          .Include(r => r.Artists).AsAsyncEnumerable()) {
      var artistSuffix = release.Artists.Any() ? $" ({release.Artists.First().Name})" : "";
      Console.WriteLine($"{release.Title}{artistSuffix}");
      foreach (var medium in release.Media) {
        foreach (var track in medium.Tracks) {
          var recording = track.Recording;
          var workSuffix = recording.Works.Any() ? $" ({recording.Works.First().Title})" : "";
          Console.WriteLine($"  {medium.Position}-{track.Position}: {recording.Title}{workSuffix}");
        }
      }
    }
  }

  private static async Task ListArtistsAsync(PartialMusicBrainzDatabase database) {
    var artists = database.Db.Artists.Select(a => new{ Artist = a, Recordings = a.Recordings.Count, Works = a.RelatedWorks.Count});
    await foreach (var a in artists.AsAsyncEnumerable()) {
      Console.WriteLine($"{a.Artist.Name} ({a.Recordings} recordings, {a.Works} works)");
    }
  }

  private enum EntityType {
    Releases,
    Artists,
    Works
  }

  public static ListCommand Register(RootCommand rootCommand, GlobalOptions globalOptions, IServiceProvider services) {
    var listCommand = new ListCommand(globalOptions, services);
    rootCommand.Add(listCommand);
    return listCommand;
  }
}