using Bogus;
using CriusNyx.Util;
using DeepEqual.Syntax;
using Sol.AST;
using Sol.DataStructures;
using Sol.Parser;
using Superpower;
using static Sol.AST.ASTBuilder;

namespace Sol.Tests;

public class ParserTests
{
  static string RandomIdent()
  {
    return new Faker().Random.String(8, 'a', 'z');
  }

  void AssertASTCompare(ASTNode actual, ASTNode expected)
  {
    try
    {
      actual
        .WithDeepEqual(expected)
        .IgnoreProperty(
          (prop) => prop.DeclaringType.IsAssignableTo(typeof(ASTNode)) && prop.Name == "NodeType"
        )
        .IgnoreProperty((prop) => prop.DeclaringType == typeof(SourceSpan) && prop.Name == "Span")
        .IgnoreProperty((prop) => prop.DeclaringType == typeof(SourceSpan) && prop.Name == "Source")
        .WithCustomComparison(new TextSpanComparison())
        .WithCustomComparison(new SpanComparison())
        .Assert();
    }
    catch
    {
      Console.WriteLine("actual:");
      Console.WriteLine(actual.Debug());
      Console.WriteLine("expected:");
      Console.WriteLine(expected.Debug());
      throw;
    }
  }

  [Test]
  public void CanParseZeroLiteral()
  {
    var expected = Prog(NumLit("0"));
    var actual = SolParser.Parse("0").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseIntLiteral()
  {
    var expected = Prog(NumLit("1"));
    var actual = SolParser.Parse("1").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseNegativeIntLiteral()
  {
    var expected = Prog(Unary("-", NumLit("1")));
    var actual = SolParser.Parse("-1").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDecimalLiteral()
  {
    var expected = Prog(NumLit("0.5"));
    var actual = SolParser.Parse("0.5").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseNegativeDecimalLiteral()
  {
    var expected = Prog(Unary("-", NumLit("0.5")));
    var actual = SolParser.Parse("-0.5").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseIdent()
  {
    var expected = Prog(LHE("value"));
    var actual = SolParser.Parse("value").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeref()
  {
    var expected = Prog(LHE("value", Deref("field")));
    var actual = SolParser.Parse("value.field").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeref_WithError1()
  {
    var expected = Prog(LHE("value", Deref(null!)));
    var (actual, context) = SolParser.ParseWithContext("value.", SolParser.ProgramParser);
    AssertASTCompare(actual, expected);

    // TODO: Errors
  }

  [Test]
  public void CanParseDeref_WithError2()
  {
    var expected = Prog(LHE("value", Deref(null!), Deref(null!)));
    var (actual, context) = SolParser.ParseWithContext("value..", SolParser.ProgramParser);
    AssertASTCompare(actual, expected);

    // TODO: Errors
  }

  [Test]
  public void CanParseMultiDeref()
  {
    var expected = Prog(LHE("value", Deref("a"), Deref("b")));
    var actual = SolParser.Parse("value.a.b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMultiDeref_WithError()
  {
    var expected = Prog(LHE("value", Deref("a"), Deref(null!)));
    var (actual, context) = SolParser.ParseWithContext("value.a.", SolParser.ProgramParser);
    AssertASTCompare(actual, expected);

    // TODO: Errors
  }

  [Test]
  public void CanParseDeindex()
  {
    var expected = Prog(LHE("value", Deindex(NumLit("0"))));
    var actual = SolParser.Parse("value[0]").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeindex_WithError1()
  {
    var expected = Prog(LHE("value", Deindex(null!, rightBracket: "")));
    var (actual, context) = SolParser.ParseWithContext("value[");

    AssertASTCompare(actual, expected);

    // TODO: errors
  }

  [Test]
  public void CanParseDeindex_WithError2()
  {
    var expected = Prog(LHE("value", Deindex(null!)));
    var (actual, context) = SolParser.ParseWithContext("value[]");

    AssertASTCompare(actual, expected);

    // TODO: errors
  }

  [Test]
  public void CanParseDeindex_WithError3()
  {
    var expected = Prog(LHE("value", Deindex(LHE("a", Deref(null!)))));
    var (actual, context) = SolParser.ParseWithContext("value[a.]");

    AssertASTCompare(actual, expected);

    // TODO: errors
  }

  [Test]
  public void CanParseInvocation()
  {
    var expected = Prog(LHE("value", Invoke()));
    var actual = SolParser.Parse("value()").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocation_WithError()
  {
    var expected = Prog(LHE("value", Invoke("(", "", [])));
    var (actual, context) = SolParser.ParseWithContext("value(");

    AssertASTCompare(actual, expected);

    // TODO: Errors
  }

  [Test]
  public void CanParseInvocationWithArg()
  {
    var expected = Prog(LHE("value", Invoke(LHE("a"))));
    var actual = SolParser.Parse("value(a)").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithArg_WithError1()
  {
    var expected = Prog(LHE("value", Invoke("(", "", LHE("a", Deref(null!)))));
    var (actual, context) = SolParser.ParseWithContext("value(a.");

    AssertASTCompare(actual, expected);

    // TODO: Errors
  }

  [Test]
  public void CanParseInvocationWithArg_WithError2()
  {
    var expected = Prog(LHE("value", Invoke(LHE("a", Deref(null!)))));
    var (actual, context) = SolParser.ParseWithContext("value(a.)");

    AssertASTCompare(actual, expected);

    // TODO: Errors
  }

  [Test]
  public void CanParseInvocationWithMultipleArg()
  {
    var expected = Prog(LHE("value", Invoke(LHE("a"), LHE("b"))));
    var actual = SolParser.Parse("value(a, b)").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithMultipleArg_WithError()
  {
    var expected = Prog(LHE("value", Invoke("(", "", LHE("a"), null!)));
    var actual = SolParser.Parse("value(a,)").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithDeref()
  {
    var expected = Prog(LHE("value", Invoke(), Deref("field")));
    var actual = SolParser.Parse("value().field").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithDeref_WithError()
  {
    var expected = Prog(LHE("value", Invoke(), Deref(null!)));
    var actual = SolParser.Parse("value().").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithDeindex()
  {
    var expected = Prog(LHE("value", Invoke(), Deindex(NumLit("0"))));
    var actual = SolParser.Parse("value()[0]").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithDeindex_WithError1()
  {
    var expected = Prog(LHE("value", Invoke(), Deindex(null!, rightBracket: "]")));
    var actual = SolParser.Parse("value()[").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithDeindex_WithError2()
  {
    var expected = Prog(LHE("value", Invoke(), Deindex(null!)));
    var actual = SolParser.Parse("value()[]").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeindexWithInvocation()
  {
    var expected = Prog(LHE("value", Deindex(NumLit("0")), Invoke()));
    var actual = SolParser.Parse("value[0]()").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeindexWithInvocation_WithError()
  {
    var expected = Prog(LHE("value", Deindex(NumLit("0")), Invoke("(", "")));
    var actual = SolParser.Parse("value[0](").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeindexWithDeref()
  {
    var expected = Prog(LHE("value", Deindex(NumLit("0")), Deref("field")));
    var actual = SolParser.Parse("value[0].field").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeindexWithDeref_WithError()
  {
    var expected = Prog(LHE("value", Deindex(NumLit("0")), Deref(null!)));
    var actual = SolParser.Parse("value[0].").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDerefWithInvocation()
  {
    var expected = Prog(LHE("value", Deref("field"), Invoke()));
    var actual = SolParser.Parse("value.field()").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDerefWithInvocation_WithError()
  {
    var expected = Prog(LHE("value", Deref("field"), Invoke("(", "")));
    var actual = SolParser.Parse("value.field(").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void FuzzDerefChain()
  {
    List<(Func<LeftHandExpressionChain?, LeftHandExpressionChain> func, string source)> list =
      new List<(Func<LeftHandExpressionChain?, LeftHandExpressionChain>, string)>();

    Random random = new Random();

    for (int i = 0; i < 1000; i++)
    {
      var rand = random.Next() % 3;
      switch (rand)
      {
        case 0:
          var ident = RandomIdent();
          list.Add((Deref(ident), $".{ident}"));
          break;
        case 1:
          list.Add((Invoke(), "()"));
          break;
        case 2:
          var index = random.Next();
          list.Add((Deindex(NumLit(index.ToString())), $"[{index}]"));
          break;
      }
    }
    var source = $"value{list.Select(x => x.source).StringJoin()}";

    var expected = Prog(LHE("value", list.Select(x => x.func).ToArray()));
    var actual = SolParser.Parse(source).Unwrap();

    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseBooleanNegate()
  {
    var expected = Prog(Unary("!", LHE("a")));
    var actual = SolParser.Parse("!a").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseBooleanNegate_WithError()
  {
    var expected = Prog(Unary("!", null!));
    var actual = SolParser.Parse("!").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseRealNegate()
  {
    var expected = Prog(Unary("-", LHE("a")));
    var actual = SolParser.Parse("-a").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseRealNegate_WithError()
  {
    var expected = Prog(Unary("-", null!));
    var actual = SolParser.Parse("-").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAdd()
  {
    var expected = Prog(Binary("+", LHE("a"), LHE("b")));
    var actual = SolParser.Parse("a + b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAdd_WithError1()
  {
    var expected = Prog(Binary("+", LHE("a"), null!));
    var actual = SolParser.Parse("a + ").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAdd_WithError2()
  {
    var expected = Prog(Binary("+", LHE("a"), LHE("b", Deref(null!))));
    var actual = SolParser.Parse("a + b.").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAdd_WithError3()
  {
    var expected = Prog(Binary("+", LHE("a", Deref(null!)), LHE("b")));
    var actual = SolParser.Parse("a. + b").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAdd_WithLit()
  {
    var expected = Prog(Binary("+", NumLit("1"), NumLit("2")));
    var actual = SolParser.Parse("1 + 2").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseSub()
  {
    var expected = Prog(Binary("-", LHE("a"), LHE("b")));
    var actual = SolParser.Parse("a - b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseSub_WithError1()
  {
    var expected = Prog(Binary("-", LHE("a"), null!));
    var actual = SolParser.Parse("a - ").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseSub_WithError2()
  {
    var expected = Prog(Binary("-", LHE("a", Deref(null!)), LHE("b")));
    var actual = SolParser.Parse("a. - b").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseSub_WithError3()
  {
    var expected = Prog(Binary("-", LHE("a"), LHE("b", Deref(null!))));
    var actual = SolParser.Parse("a - b.").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDoubleSub()
  {
    var expected = Prog(Binary("-", LHE("a"), Unary("-", LHE("b"))));
    var actual = SolParser.Parse("a - - b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDoubleSub_WithError1()
  {
    var expected = Prog(Binary("-", LHE("a"), Unary("-", null!)));
    var actual = SolParser.Parse("a - - ").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDoubleSub_WithError2()
  {
    var expected = Prog(Binary("-", LHE("a"), Unary("-", LHE("b", Deref(null!)))));
    var actual = SolParser.Parse("a - - b.").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMul()
  {
    var expected = Prog(Binary("*", LHE("a"), LHE("b")));
    var actual = SolParser.Parse("a * b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMul_WithError1()
  {
    var expected = Prog(Binary("*", LHE("a"), null!));
    var actual = SolParser.Parse("a * ").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMul_WithError2()
  {
    var expected = Prog(Binary("*", LHE("a"), LHE("b", Deref(null!))));
    var actual = SolParser.Parse("a * b.").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMul_WithError3()
  {
    var expected = Prog(Binary("*", LHE("a", Deref(null!)), LHE("b")));
    var actual = SolParser.Parse("a. * b").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDiv()
  {
    var expected = Prog(Binary("/", LHE("a"), LHE("b")));
    var actual = SolParser.Parse("a / b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDiv_WithError1()
  {
    var expected = Prog(Binary("/", LHE("a"), null!));
    var actual = SolParser.Parse("a / ").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDiv_WithError2()
  {
    var expected = Prog(Binary("/", LHE("a", Deref(null!)), LHE("b")));
    var actual = SolParser.Parse("a. / b").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDiv_WithError3()
  {
    var expected = Prog(Binary("/", LHE("a"), LHE("b", Deref(null!))));
    var actual = SolParser.Parse("a / b.").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMod()
  {
    var expected = Prog(Binary("%", LHE("a"), LHE("b")));
    var actual = SolParser.Parse("a % b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMod_WithError1()
  {
    var expected = Prog(Binary("%", LHE("a"), null!));
    var actual = SolParser.Parse("a % ").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMod_WithError2()
  {
    var expected = Prog(Binary("%", LHE("a", Deref(null!)), LHE("b")));
    var actual = SolParser.Parse("a. % b").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMod_WithError3()
  {
    var expected = Prog(Binary("%", LHE("a"), LHE("b", Deref(null!))));
    var actual = SolParser.Parse("a % b.").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void OpsAreOrderedCorrectly()
  {
    var expected = Prog(
      Binary("+", Binary("*", LHE("a"), LHE("b")), Binary("/", LHE("c"), LHE("d")))
    );
    var actual = SolParser.Parse("a * b + c / d").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseLeadingMinus()
  {
    var expected = Prog(
      Binary("+", Binary("*", Unary("-", LHE("a")), LHE("b")), Binary("/", LHE("c"), LHE("d")))
    );
    var actual = SolParser.Parse("-a * b + c / d").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAssign()
  {
    var expected = Prog(Assign(LHE("a"), LHE("b")));
    var actual = SolParser.Parse("a = b").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseUse()
  {
    var expected = Prog(Use(Ident("System")));
    var actual = SolParser.Parse("use System").Unwrap();
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseUse_WithError()
  {
    var expected = Prog(UseExplicit(null!));
    var actual = SolParser.Parse("use ").Error.RecoverAST();
    AssertASTCompare(actual, expected);
  }
}
