using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Clsfy.Database;
using Clsfy.MusicBrainz;
using Clsfy.MusicBrainz.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Clsfy.CLI;

class Program {
  static async Task Main(string[] args) {
    await BuildCommandLine()
          .UseHost(
            _ => Host.CreateDefaultBuilder(),
            host => {
              host.ConfigureServices(
                    services => services
                                .AddSingleton<Program>()
                                .AddTransient<PartialMusicBrainzDatabase>()
                                .AddDbContext<MusicBrainzContext>(o => { o.UseSqlite("Data Source='test.sqlite'"); })
                                .AddTransient<IQueryWrapper, QueryWrapper>()
                                .AddTransient<IClsfyMusicBrainzClient, ClsfyMusicBrainzClient>()
                                .AddSingleton<RecordingRelationFactory>()
                  )
                  .ConfigureLogging(
                    (_, logging) => logging
                                    .ClearProviders()
                                    .AddSimpleConsole(options => options.IncludeScopes = true)
                                    .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning)
                                    .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                  );
            })
          .UseDefaults()
          .Build()
          .InvokeAsync(args);
  }

  private static CommandLineBuilder BuildCommandLine() {
    var root = new RootCommand {
      new Option<string>("--server", () => "http://localhost:5000", "the musicbrainz server to connect to")
    };
    //root.Handler = CommandHandler.Create<Options, IHost>(Run);
    root.AddCommand(new AddCommand());
    root.AddCommand(new ListCommand());
    return new CommandLineBuilder(root);
  }

  private static void Run(Options options, IHost host) {

  }
}
/*
Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddCommandLine(args))
    .ConfigureServices((_, services) =>
    {
      services.AddHostedService<ConsoleHostedService>();
      var query = new Query("clsfy", "0.1.0", "michaelhelmling@posteo.de");

      var uri = new Uri("http://localhost:5000");
      query.Server = uri.Host;
      query.Port = uri.Port;
      query.UrlScheme = uri.Scheme;
      Query.DelayBetweenRequests = 0;

      services.AddSingleton(query);
      services.AddSingleton<PartialMusicBrainzDatabase>();
    })
    .ConfigureLogging((_, logging) =>
    {
      logging.ClearProviders();
      logging.AddSimpleConsole(options => options.IncludeScopes = true);
      logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
    });

*/
