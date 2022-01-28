using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Clsfy.Database;
using Microsoft.Extensions.Hosting;

namespace Clsfy.CLI;

public class AddCommand : ClsfyCommand<AddCommand.Options> {
  public enum EntityType {
    Release,
  }

  public record Options(EntityType Entity, Guid Mbid);
  public AddCommand() : base("add") {
    var entityArgument = new Argument<EntityType>("entity", "the type of entity to add");
    var mbidArgument = new Argument<Guid>("mbid", "the musicbrainz id of the entity");
    AddArgument(entityArgument);
    AddArgument(mbidArgument);
  }

  protected override async Task HandleAsync(CLI.Options globalOptions, Options options, IHost host) {
    var database = host.Services.GetRequiredService<PartialMusicBrainzDatabase>();
    await database.GetOrAddRelease(options.Mbid);
  }
}
