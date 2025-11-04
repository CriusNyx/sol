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
    var compiled = Compiler.TypeCheck(source).Unwrap();
    var astString = compiled.AST.Debug();
    var typeString = compiled.AST.FormatWithTypes();
    var expectedAST = File.ReadAllText(path.Replace(Path.GetExtension(path), ".ast"));
    var expectedTypes = File.ReadAllText(path.Replace(Path.GetExtension(path), ".types"));
    Assert.That(astString, Is.EqualTo(expectedAST));
    Assert.That(typeString, Is.EqualTo(expectedTypes));
  }
}
