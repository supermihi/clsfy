using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Clsfy.Database;
using Microsoft.Extensions.Hosting;

namespace Clsfy.CLI;

public class ListCommand : ClsfyCommand<ListCommand.CommandOptions> {
  public record CommandOptions(EntityType Entity);

  public enum EntityType {
    Releases,
    Artists,
    Works
  }

  public ListCommand() : base("list") {
    AddArgument(new Argument<EntityType>("entity", "the type of entity to list"));
  }

  protected override async Task HandleAsync(CLI.Options global, CommandOptions options, IHost host) {
    var database = host.Services.GetRequiredService<PartialMusicBrainzDatabase>();
    switch (options.Entity) {
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
        throw new ArgumentOutOfRangeException(nameof(options.Entity), options.Entity, null);
    }
  }

  private async Task ListWorksAsync(PartialMusicBrainzDatabase database) {
    var topLevelWorks = database.Db.Works.Where(work => !work.Containing.Any());
    await foreach (var work in topLevelWorks.Include(w => w.Artists).AsAsyncEnumerable()) {
      Console.WriteLine($"{work.Title} by {string.Join(',', work.Artists.Select(a => a.Name))}");
    }
  }

  private static async Task ListReleasesAsync(PartialMusicBrainzDatabase database) {
    await foreach (var release in database.Db.Releases.Include(r => r.Media.OrderBy(m => m.Position))
                                          .ThenInclude(m => m.Tracks.OrderBy(t => t.Position))
                                          .ThenInclude(t => t.Recording).ThenInclude(r => r.Works)
                                          .ThenInclude(w => w.ArtistRelations)
                                          .Include(r => r.Artists).AsAsyncEnumerable()) {
      var artistSuffix = release.Artists.Any() ? $" ({release.Artists.First().Name})" : "";
      Console.WriteLine($"{release.Title}{artistSuffix}");
      foreach (var medium in release.Media) {
        foreach (var track in medium.Tracks) {
          var recording = track.Recording;
          var work = recording.Works.FirstOrDefault();
          string title;
          if (work is null) {
            title = recording.Title;
          }
          else {
            var artistRel = work.ArtistRelations.ToList();
            title = $"* {work.Title} ({string.Join(", ", artistRel.Select(a => a.Artist.Name))})";
          }
          Console.WriteLine($"  {medium.Position}-{track.Position}: {title}");
        }
      }
    }
  }

  private static async Task ListArtistsAsync(PartialMusicBrainzDatabase database) {
    var artists =
      database.Db.Artists.Select(a => new {
        Artist = a, Recordings = a.Recordings.Count, Works = a.RelatedWorks.Count
      });
    await foreach (var a in artists.AsAsyncEnumerable()) {
      Console.WriteLine($"{a.Artist.Name} ({a.Recordings} recordings, {a.Works} works)");
    }
  }
}
