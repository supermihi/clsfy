using System.CommandLine;
using MusicBrainz.Partial;

namespace Clsfy.CLI; 

public class ListCommand : Command {

  public ListCommand(Option<string> dbOption)  : base("list") {
    var entityArgument = new Argument<EntityType>("entity", "the type of entity to list");
    AddArgument(entityArgument);
    this.SetHandler<string, EntityType>(Handle, dbOption, entityArgument);
  }

  private void Handle(string database, EntityType entityType) => HandleAsync(database, entityType).GetAwaiter().GetResult();

  private async Task HandleAsync(string databasePath, EntityType entityType) {
    var database = DatabaseFactory.Create(databasePath);
    await foreach (var release in database.GetReleasesAsync()) {
      Console.WriteLine(release.Title);
    }
  }

  private enum EntityType {
    Releases,
    Works
  }

  public static ListCommand Register(RootCommand rootCommand, Option<string> dbOption) {
    var listCommand = new ListCommand(dbOption);
    rootCommand.Add(listCommand);
    return listCommand;
  }
  
}