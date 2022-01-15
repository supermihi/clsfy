using System.CommandLine;
using Clsfy.CLI;

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

ListCommand.Register(rootCommand, globalOptions);
AddCommand.Register(rootCommand, globalOptions);
return await rootCommand.InvokeAsync(args);

