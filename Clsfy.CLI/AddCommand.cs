using System.CommandLine;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MusicBrainz.Partial;

namespace Clsfy.CLI; 

public class AddCommand  : Command {

  public AddCommand(Option<string> dbOption) : base("add") {
    var entityArgument = new Argument<EntityType>("entity", "the type of entity to add");
    var mbidArgument = new Argument<Guid>("mbid", "the musicbrainz id of the entity");
    AddArgument(entityArgument);
    AddArgument(mbidArgument);
    this.SetHandler<string, EntityType, Guid>(Handle, dbOption, entityArgument, mbidArgument);
  }

  private enum EntityType {
    Release,
  }

  private void Handle(string dbPath, EntityType entityType, Guid mbid) =>
      HandleAsync(dbPath, entityType, mbid).GetAwaiter().GetResult();

  private async Task HandleAsync(string dbPath, EntityType entityType, Guid mbid) {
    var database = DatabaseFactory.Create(dbPath);
    await database.AddRelease(mbid);
  }

  public static AddCommand Register(RootCommand rootCommand, Option<string> dbOption) {
    var addCommand = new AddCommand(dbOption);
    rootCommand.AddCommand(addCommand);
    return addCommand;
  }
}