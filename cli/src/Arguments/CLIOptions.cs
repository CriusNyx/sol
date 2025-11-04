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

  [Option("ast", Required = false, Default = false, HelpText = "Print program AST")]
  public bool AST { get; set; }

  [Option("types", Required = false, Default = false, HelpText = "Print program types")]
  public bool Types { get; set; }

  [Option(
    "generate-test-files",
    Required = false,
    Default = false,
    HelpText = "Generate test files"
  )]
  public bool GenerateTestfiles { get; set; }
}
