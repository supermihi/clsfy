using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using MusicBrainz.Partial;

namespace Clsfy.CLI;

public class ListCommand : Command {
  public ListCommand(GlobalOptions options) : base("list") {
    var entityArgument = new Argument<EntityType>("entity", "the type of entity to list");
    AddArgument(entityArgument);
    this.SetHandler<string, string?, EntityType>(Handle, options.Database, options.Server, entityArgument);
  }

  private void Handle(string database, string? server, EntityType entityType) =>
      HandleAsync(database, server, entityType).GetAwaiter().GetResult();

  private async Task HandleAsync(string databasePath, string? server, EntityType entityType) {
    var database = DatabaseFactory.Create(databasePath, server);
    switch (entityType) {
      case EntityType.Releases:
        await ListReleasesAsync(database);
        break;
      case EntityType.Artists:
        await ListArtistsAsync(database);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
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
    await foreach (var artist in database.Db.Artists.AsAsyncEnumerable()) {
      Console.WriteLine(artist.Name);
    }
  }

  private enum EntityType {
    Releases,
    Artists
  }

  public static ListCommand Register(RootCommand rootCommand, GlobalOptions globalOptions) {
    var listCommand = new ListCommand(globalOptions);
    rootCommand.Add(listCommand);
    return listCommand;
  }
}