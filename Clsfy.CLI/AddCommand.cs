using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicBrainz.Partial;

namespace Clsfy.CLI; 

public class AddCommand  : Command {
  private readonly IServiceProvider _serviceProvider;

  public AddCommand(GlobalOptions options, IServiceProvider serviceProvider) : base("add") {
    _serviceProvider = serviceProvider;
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
    var database = DatabaseFactory.Create(dbPath, server, _serviceProvider.GetService<ILoggerFactory>());
    await database.AddRelease(mbid);
  }

  public static AddCommand Register(RootCommand rootCommand, GlobalOptions options, IServiceProvider services) {
    var addCommand = new AddCommand(options, services);
    rootCommand.AddCommand(addCommand);
    return addCommand;
  }
}