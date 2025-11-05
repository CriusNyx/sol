using CriusNyx.Util;
using Sol.AST;
using Sol.DataStructures;

namespace Sol.Tests;

public class TestProgramTests
{
  [DatapointSource]
  public string[] Paths => Directory.GetFiles("testPrograms", "*.sol");

  [Theory]
  public void TestProgramConsistancy(string path)
  {
    var source = File.ReadAllText(path);
    var compiledResult = Compiler.TypeCheck(source);
    var ast = compiledResult.Map(x => x.AST).UnwrapOrElse(x => x.RecoverAST());

    var astString = ast.Debug();
    var typeString = ast.FormatWithTypes();

    var expectedAST = File.ReadAllText(path.Replace(Path.GetExtension(path), ".ast"));
    var expectedTypes = File.ReadAllText(path.Replace(Path.GetExtension(path), ".types"));

    Assert.That(astString, Is.EqualTo(expectedAST));
    Assert.That(typeString, Is.EqualTo(expectedTypes));
  }
}
