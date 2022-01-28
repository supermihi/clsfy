using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;

namespace Clsfy.CLI;

public abstract class ClsfyCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions> : Command {
  protected ClsfyCommand(string name) : base(name) {
    Handler = CommandHandler.Create<Options, TOptions, IHost>(Handle);
  }
  private void Handle(Options globalOptions, TOptions options, IHost host) =>
    HandleAsync(globalOptions, options, host).GetAwaiter().GetResult();

  protected abstract Task HandleAsync(Options globalOtions, TOptions options, IHost host);

}
