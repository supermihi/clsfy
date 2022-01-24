using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Clsfy.CLI;
using Clsfy.Database;
using MetaBrainz.MusicBrainz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program {
  static async Task Main(string[] args) {
    await BuildCommandLine()
          .UseHost(_ => Host.CreateDefaultBuilder(),
                   host => { host.ConfigureServices(services => { services.AddSingleton<Program>(); }); })
          .UseDefaults()
          .Build()
          .InvokeAsync(args);
  }

  private static CommandLineBuilder BuildCommandLine() {
    var root = new RootCommand(@"$ dotnet run --name 'Joe'") {
        new Option<string>("--name") {
            IsRequired = true
        }
    };
    root.Handler = CommandHandler.Create<Options, IHost>(Run);
    return new CommandLineBuilder(root);
  }

  private static void Run(Options options, IHost host) {
    var serviceProvider = host.Services;
    var greeter = serviceProvider.GetRequiredService<IGreeter>();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger(typeof(Program));

    var name = options.Name;
    logger.LogInformation(GreetEvent, "Greeting was requested for: {name}", name);
    greeter.Greet(name);
  }
}

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

