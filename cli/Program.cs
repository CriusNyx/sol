using System.Drawing;
using CommandLine;
using CriusNyx.Util;
using Pastel;
using Sol;
using Sol.AST;
using Sol.CLI;
using Sol.DataStructures;
using Superpower;
using SColor = System.Drawing.Color;

SColor keyword = Hex("#569cd6");
SColor field = Hex("#9cdcfe");
SColor className = Hex("#4ec9b0");
SColor method = Hex("#dcdcaa");
SColor stringLit = Hex("#ce9178");
SColor numLit = Hex("#b5cea8");

var options = Parser.Default.ParseArguments<CLIOptions>(args).Value;

if (options.Debugger)
{
  while (!System.Diagnostics.Debugger.IsAttached)
  {
    Thread.Sleep(500);
  }
}

if (options.Pretty)
{
  PrintPretty(options.Files);
}
else if (options.AST)
{
  PrintAST(options.Files);
}
else if (options.Types)
{
  PrintTypes(options.Files);
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
  Evaluate(options.Files);
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

void PrintPretty(IEnumerable<string> files)
{
  ForFiles(
    files,
    (fileArgs) =>
    {
      void PrintAST(ASTNode ast, string source)
      {
        var semanticStream = ast.GetSemantics().Stream(source);
        foreach (var (segment, token) in semanticStream)
        {
          Console.Write(Color(segment, token));
        }
      }

      var (_, source) = fileArgs;
      var parsed = Compiler.TypeCheck(source);
      if (parsed.IsSuccess)
      {
        PrintAST(parsed.Value.AST, source);
      }
      else if (parsed.Error is PartialCompileError partial)
      {
        PrintAST(partial.AST, source);
      }
      else
      {
        Console.WriteLine("Error");
      }
      // Add new line at end of program.
      Console.WriteLine("");
    }
  );
}

void PrintAST(IEnumerable<string> files)
{
  ForFiles(
    files,
    (fileArgs) =>
    {
      var (_, source) = fileArgs;
      var compiled = Compiler.Parse(source);
      Console.WriteLine(compiled.Value.AST.Debug());
    }
  );
}

void PrintTypes(IEnumerable<string> files)
{
  ForFiles(
    files,
    (fileArgs) =>
    {
      var (_, source) = fileArgs;
      var compiled = Compiler.TypeCheck(source);
      Console.WriteLine(compiled.Unwrap().AST.FormatWithTypes());
    }
  );
}

void StartInteractive()
{
  new InteractiveInterface().Run();
}

void Evaluate(IEnumerable<string> files)
{
  ForFiles(
    files,
    (file) =>
    {
      var result = Compiler.Evaluate(file.source);
      if (result.IsSuccess)
      {
        Console.WriteLine(result.Value.Result?.Debug());
      }
      else
      {
        Console.WriteLine(result.Error);
      }
    }
  );
}

Result<GenreateTestResult, Exception> GenerateTestFile(string path, string source)
{
  try
  {
    var result = Compiler.TypeCheck(source);
    var ast = result.Map(x => x.AST).UnwrapOrElse((err) => err.RecoverAST());
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
  if (results.All(x => x.IsSuccess))
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
        if (result.Safe(r => r.Error) is Exception e)
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

void ForFiles(IEnumerable<string> files, Action<(string path, string source)> action)
{
  foreach (var (path, source) in FilesWithSource(files))
  {
    Console.WriteLine(path);
    Console.WriteLine("");
    action((path, source));
    Console.WriteLine("");
  }
}

class GenreateTestResult(string path, string source, string astDebug, string astTypes)
{
  public string Path => path;
  public string Source => source;
  public string ASTDebug => astDebug;
  public string ASTTypes => astTypes;
}
