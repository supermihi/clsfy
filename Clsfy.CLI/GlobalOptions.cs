using System.CommandLine;

namespace Clsfy.CLI; 

public record GlobalOptions(Option<string> Database, Option<string?> Server);