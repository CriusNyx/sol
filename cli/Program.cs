using System.Drawing;
using CommandLine;
using CriusNyx.Util;
using Pastel;
using Sol.CLI;
using SColor = System.Drawing.Color;

SColor keyword = Hex("#569cd6");
SColor field = Hex("#9cdcfe");
SColor className = Hex("#4ec9b0");
SColor method = Hex("#dcdcaa");
SColor stringLit = Hex("#ce9178");
SColor numLit = Hex("#b5cea8");

var options = Parser.Default.ParseArguments<CLIOptions>(args).Value;

options = new CLIOptions { Pretty = true, Files = ["resources/Test.sol"] };

if (options.Pretty)
{
  PrintPretty(options.Files);
}
if (options.Interactive)
{
  StartInteractive();
}

SColor Hex(string hex)
{
  return ColorTranslator.FromHtml(hex);
}

string Color(string source, SemanticToken token)
{
  switch (token.Type)
  {
    case SemanticType.None:
      return source;
    case SemanticType.Keyword:
      return source.Pastel(keyword);
    case SemanticType.ClassReference:
      return source.Pastel(className);
    case SemanticType.MethodReference:
      return source.Pastel(method);
    case SemanticType.ObjectReference:
      return source.Pastel(field);
    case SemanticType.NumLit:
      return source.Pastel(numLit);
    case SemanticType.StringLit:
      return source.Pastel(stringLit);

    default:
      throw new NotImplementedException();
  }
}

void PrintPretty(IEnumerable<string> files)
{
  var filesWithSource = files.Select(file => file.With(File.ReadAllText(file)));
  foreach (var (path, source) in filesWithSource)
  {
    Console.WriteLine(path);
    Console.WriteLine("");
    var parsed = Compiler.TypeCheck(source);
    if (parsed.IsSuccess)
    {
      var ast = parsed.Value.AST;
      var semanticStream = ast.Semantics().Stream(source);
      foreach (var (segment, token) in semanticStream)
      {
        Console.Write(Color(segment, token));
      }
    }
    else
    {
      Console.WriteLine("Error");
    }
    Console.WriteLine("");
  }
}

void StartInteractive()
{
  new InteractiveInterface().Run();
}
