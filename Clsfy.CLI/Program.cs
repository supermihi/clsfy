using Clsfy.CLI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await CreateHostBuilder(args).RunConsoleAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder => builder.AddCommandLine(args))
        .ConfigureServices((_, services) =>
        {
          services.AddHostedService<ConsoleHostedService>();
        })
        .ConfigureLogging((_, logging) => 
        {
          logging.ClearProviders();
          logging.AddSimpleConsole(options => options.IncludeScopes = true);
          logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
        });

