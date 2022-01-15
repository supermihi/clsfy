using System.CommandLine;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MusicBrainz.Partial;

namespace Clsfy.CLI; 

public class AddCommand  : Command {

  public AddCommand(GlobalOptions options) : base("add") {
    var entityArgument = new Argument<EntityType>("entity", "the type of entity to add");
    var mbidArgument = new Argument<Guid>("mbid", "the musicbrainz id of the entity");
    AddArgument(entityArgument);
    AddArgument(mbidArgument);
    this.SetHandler<string, string?, EntityType, Guid>(Handle, options.Database, options.Server, entityArgument, mbidArgument);
  }

  private enum EntityType {
    Release,
  }

  private void Handle(string dbPath, string? server, EntityType entityType, Guid mbid) =>
      HandleAsync(dbPath, server, entityType, mbid).GetAwaiter().GetResult();

  private async Task HandleAsync(string dbPath, string? server, EntityType? entityType, Guid mbid) {
    var database = DatabaseFactory.Create(dbPath, server);
    await database.AddRelease(mbid);
  }

  public static AddCommand Register(RootCommand rootCommand, GlobalOptions options) {
    var addCommand = new AddCommand(options);
    rootCommand.AddCommand(addCommand);
    return addCommand;
  }
}