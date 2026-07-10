using System.Diagnostics;
using System.Drawing;
using CriusNyx.Results;
using CriusNyx.Util;
using DevCon;
using DevCon.AST;
using DevCon.CLI;
using Pastel;
using Superpower;
using SColor = System.Drawing.Color;

SColor keyword = Hex("#569cd6");
SColor field = Hex("#9cdcfe");
SColor className = Hex("#4ec9b0");
SColor method = Hex("#dcdcaa");
SColor stringLit = Hex("#ce9178");
SColor numLit = Hex("#b5cea8");

var idk = Console.IsInputRedirected;

var options = CLIOptions.Parse(args);

if (options.Debugger)
{
  while (!Debugger.IsAttached)
  {
    Thread.Sleep(500);
  }
}

var semantics = Compiler.TypeCheck("use").Err().Unwrap().ast.GetSemantics().ToArray();

if (options.Pretty)
{
  PrintPretty(LoadSourceCode());
}
else if (options.AST)
{
  PrintAST(LoadSourceCode());
}
else if (options.Types)
{
  PrintTypes(LoadSourceCode());
}
else if (options.Interactive)
{
  StartInteractive();
}
else if (options.GenerateTestfiles)
{
  GenerateTestFiles(options.Files);
}
else
{
  Evaluate(LoadSourceCode());
}

IEnumerable<SourceCode> LoadSourceCode()
{
  if (Console.IsInputRedirected)
  {
    yield return new SourceCode(SourceCodeType.Console, "", Console.In.ReadToEnd());
  }
  foreach (var file in options.Files)
  {
    yield return new SourceCode(SourceCodeType.File, file, File.ReadAllText(file));
  }
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
    case SemanticType.ClassName:
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

void PrintPretty(IEnumerable<SourceCode> sources)
{
  void PrintAST(ASTNode ast, string source)
  {
    var semanticStream = ast.GetSemantics().Stream(source);
    foreach (var (segment, token) in semanticStream)
    {
      Console.Write(Color(segment, token));
    }
  }

  foreach (var source in sources)
  {
    var parsed = Compiler.TypeCheck(source.Source);
    if (parsed.IsOk())
    {
      PrintAST(parsed.Unwrap().AST, source.Source);
    }
    else if (parsed.Err().Unwrap() is CompilerError partial)
    {
      PrintAST(partial.ast, source.Source);
    }
    else
    {
      Console.WriteLine("Error");
    }
    // Add new line at end of program.
    Console.WriteLine("");
  }
}

void PrintAST(IEnumerable<SourceCode> sources)
{
  foreach (var source in sources)
  {
    var compiled = Compiler.Parse(source.Source);
    Console.WriteLine(compiled.Unwrap().AST.Debug());
  }
}

void PrintTypes(IEnumerable<SourceCode> sources)
{
  foreach (var source in sources)
  {
    var compiled = Compiler.TypeCheck(source.Source);
    Console.WriteLine(compiled.Unwrap().AST.FormatWithTypes());
  }
}

void StartInteractive()
{
  new InteractiveInterface().Run();
}

void Evaluate(IEnumerable<SourceCode> sources)
{
  foreach (var source in sources)
  {
    var result = Compiler.Evaluate(source.Source);
    if (result.IsOk())
    {
      Console.WriteLine(result.Unwrap().Result?.Debug());
    }
    else
    {
      Console.WriteLine(result.Err().Unwrap());
    }
  }
}

Result<GenreateTestResult, Exception> GenerateTestFile(string path, string source)
{
  try
  {
    var result = Compiler.TypeCheck(source);
    var ast = result.Map(x => x.AST).UnwrapOrElse((err) => err.ast);
    var astDebug = ast.Debug();
    var astTypes = ast.FormatWithTypes();
    return new GenreateTestResult(path, source, astDebug, astTypes);
  }
  catch (Exception e)
  {
    return e;
  }
}

void GenerateTestFiles(IEnumerable<string> files)
{
  var results = FilesWithSource(files).Select((pair) => GenerateTestFile(pair.path, pair.source));
  if (results.All(x => x.IsOk()))
  {
    var tasks = results.Select(x => x.Unwrap());
    Console.WriteLine("The following files will be changed".Pastel(ConsoleColor.Green));
    foreach (var task in tasks)
    {
      Console.WriteLine(task.Path);
    }
    if (!CLI.PromptYN("Overwrite test files?".Pastel(ConsoleColor.Red)))
    {
      return;
    }
    foreach (var result in results.Select(x => x.Unwrap()))
    {
      var path = result.Path;
      var baseName = Path.Join(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
      var astPath = baseName + ".ast";
      var typesPath = baseName + ".types";
      File.WriteAllText(astPath, result.ASTDebug);
      File.WriteAllText(typesPath, result.ASTTypes);
    }
  }
  else
  {
    results.Foreach(
      (result) =>
      {
        if (result.Safe(r => r.Err().Unwrap()) is Exception e)
        {
          Console.WriteLine(e);
        }
      }
    );
  }
}

IEnumerable<(string path, string source)> FilesWithSource(IEnumerable<string> files)
{
  return files.Select(file => file.With(File.ReadAllText(file)));
}

enum SourceCodeType
{
  File,
  Console,
}

class SourceCode(SourceCodeType type, string path, string source)
{
  public SourceCodeType Type => type;
  public string Path => path;
  public string Source => source;
}

class GenreateTestResult(string path, string source, string astDebug, string astTypes)
{
  public string Path => path;
  public string Source => source;
  public string ASTDebug => astDebug;
  public string ASTTypes => astTypes;
}
