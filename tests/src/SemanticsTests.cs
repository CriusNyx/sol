using System.Data;
using CriusNyx.Util;
using DeepEqual.Syntax;
using DevCon.AST;
using DevCon.DataStructures;
using DevCon.TypeSystem;

namespace DevCon.Tests;

public static class TestClass
{
  public static string StringMethod()
  {
    throw new NotImplementedException();
  }

  public static string StringMethod(string value)
  {
    throw new NotImplementedException();
  }
}

public class SemanticsTests
{
  private static SemanticToken Token(SemanticType type)
  {
    return new SemanticToken(Span.Empty, type);
  }

  private static SemanticToken[] TokenList(params SemanticType[] types)
  {
    return types.Select(x => Token(x)).ToArray();
  }

  private static ASTNode TestParse(string source, params (string, DevConType)[] parameters)
  {
    var context = new TypeContext();
    foreach (var (variable, value) in parameters)
    {
      context.typeScope.SetType(variable, value);
    }
    return Compiler.TypeCheck(source, context).Unwrap().AST;
  }

  private void SemanticTokenCompare(object actual, object expected)
  {
    actual
      .WithDeepEqual(expected)
      .IgnoreProperty(typeof(SemanticToken), nameof(SemanticToken.Span))
      .Assert();
  }

  [Test]
  public void Use_IsCorrect()
  {
    var program = TestParse("use System");

    var expected = TokenList(SemanticType.Keyword, SemanticType.ClassName);
    var actual = program.GetSemantics();

    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void Use_Multiple_IsCorrect()
  {
    var program = TestParse("use System.Diagnostics");

    var expected = TokenList(SemanticType.Keyword, SemanticType.ClassName, SemanticType.ClassName);
    var actual = program.GetSemantics();

    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void LHE_ClassRef_IsCorrect()
  {
    var lhe = TestParse("use System\nConsole")
      .FindNode((node) => node is LeftHandExpression)
      .NotNull();

    var expected = TokenList(SemanticType.ClassName);
    var actual = lhe.GetSemantics();

    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void LHE_Identifier_IsCorrect()
  {
    var lhe = TestParse("value", ("value", new CSType(typeof(TestType))))
      .FindNode((node) => node is LeftHandExpression)
      .NotNull();

    var expected = TokenList(SemanticType.ObjectReference);
    var actual = lhe.GetSemantics();

    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void LHE_MethodRef_IsCorrect()
  {
    string source = "use System\nConsole.WriteLine";
    var lhe = TestParse(source).FindNode((node) => node is LeftHandExpression).NotNull();

    var expected = TokenList(SemanticType.ClassName, SemanticType.MethodReference);
    var actual = lhe.GetSemantics();

    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void LHE_StaticMethodInvocation_IsCorrect()
  {
    string source = "use DevCon.Tests\nTestClass.StringMethod()";
    var lhe = TestParse(source).FindNode((node) => node is LeftHandExpression).NotNull();

    var expected = TokenList(SemanticType.ClassName, SemanticType.MethodReference);

    var actual = lhe.GetSemantics();
    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void LHE_StaticMethodInvocation_WithArgs_IsCorrect()
  {
    string source = "use DevCon.Tests\nTestClass.StringMethod(value)";
    var lhe = TestParse(source, ("value", new CSType(typeof(string))))
      .FindNode((node) => node is LeftHandExpression)
      .NotNull();

    var expected = TokenList(
      SemanticType.ClassName,
      SemanticType.MethodReference,
      SemanticType.ObjectReference
    );

    var actual = lhe.GetSemantics();
    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void NumLit_IsCorrect()
  {
    string source = "0";
    var result = TestParse(source);

    var expected = TokenList(SemanticType.NumLit);
    var actual = result.GetSemantics();

    SemanticTokenCompare(actual, expected);
  }

  [Test]
  public void StringLit_IsCorrect()
  {
    string source = "\"string\"";
    var result = TestParse(source);

    var expected = TokenList(SemanticType.StringLit);
    var actual = result.GetSemantics();

    SemanticTokenCompare(actual, expected);
  }
}
