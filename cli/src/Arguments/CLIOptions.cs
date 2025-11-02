using CommandLine;

namespace Sol.CLI;

public class CLIOptions
{
  [Value(0, HelpText = "The files to execute.")]
  public IEnumerable<string> Files { get; set; } = null!;

  [Option(
    'i',
    "interactive",
    Required = false,
    Default = false,
    HelpText = "Run the software in interactive mode."
  )]
  public bool Interactive { get; set; }

  [Option(
    'p',
    "pretty",
    Required = false,
    Default = false,
    HelpText = "Print pretty to the command line."
  )]
  public bool Pretty { get; set; }
}
