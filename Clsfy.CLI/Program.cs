using System.CommandLine;
using Clsfy.CLI;

var dbOption = new Option<string>(
                                  "--database",
                                  getDefaultValue: () => "test.sqlite",
                                  description: "the database file to use");
var rootCommand = new RootCommand();
rootCommand.AddOption(dbOption);
rootCommand.Description = "Clsfy Command-Line Interface";

ListCommand.Register(rootCommand, dbOption);
AddCommand.Register(rootCommand, dbOption);
return await rootCommand.InvokeAsync(args);

