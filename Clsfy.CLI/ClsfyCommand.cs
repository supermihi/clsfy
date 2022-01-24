using System.CommandLine;
using Microsoft.Extensions.Hosting;

namespace Clsfy.CLI;

public class ClsfyProgram  {
  private readonly IServiceProvider _services;
  private readonly IHostApplicationLifetime _appLifetime;
  private int _exitCode;

  public ClsfyProgram(IServiceProvider services, IHostApplicationLifetime appLifetime) {
    _services = services;
    _appLifetime = appLifetime;
  }
  public Task StartAsync(CancellationToken cancellationToken) {
    _appLifetime.ApplicationStarted.Register(() => Task.Run(Run, cancellationToken));
    return Task.CompletedTask;
  }

  private async Task Run() {
    var args = Environment.GetCommandLineArgs();
    var dbOption = new Option<string>(
                                      "--database",
                                      getDefaultValue: () => "test.sqlite",
                                      description: "the database file to use");
    var serverOption = new Option<string?>("--server", "the server to connect to");
    var rootCommand = new RootCommand();
    rootCommand.AddOption(dbOption);
    rootCommand.AddOption(serverOption);
    var globalOptions = new GlobalOptions(dbOption, serverOption);
    rootCommand.Description = "Clsfy Command-Line Interface";

    ListCommand.Register(rootCommand, globalOptions, _services);
    AddCommand.Register(rootCommand, globalOptions, _services);
    _exitCode = await rootCommand.InvokeAsync(args);
    _appLifetime.StopApplication();
  }

  public Task StopAsync(CancellationToken cancellationToken) {
    Environment.ExitCode = _exitCode;
    return Task.CompletedTask;
  }
}
