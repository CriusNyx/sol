using Bogus;
using CriusNyx.Util;
using DeepEqual.Syntax;
using Sol.AST;
using Sol.Parser;
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
    actual.WithDeepEqual(expected).WithCustomComparison(new TextSpanComparison()).Assert();
  }

  [Test]
  public void CanParseZeroLiteral()
  {
    var expected = NumLit(0);
    var actual = SolParser.Parse("0");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseIntLiteral()
  {
    var expected = NumLit(1);
    var actual = SolParser.Parse("1");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseNegativeIntLiteral()
  {
    var expected = Unary("-", NumLit(1));
    var actual = SolParser.Parse("-1");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDecimalLiteral()
  {
    var expected = NumLit(0.5m);
    var actual = SolParser.Parse("0.5");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseNegativeDecimalLiteral()
  {
    var expected = Unary("-", NumLit(0.5m));
    var actual = SolParser.Parse("-0.5");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseIdent()
  {
    var expected = LHE("value");
    var actual = SolParser.Parse("value");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeref()
  {
    var expected = LHE("value", Deref("field"));
    var actual = SolParser.Parse("value.field");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMultiDeref()
  {
    var expected = LHE("value", Deref("a"), Deref("b"));
    var actual = SolParser.Parse("value.a.b");
    AssertASTCompare(actual, expected);
  }

  public void CanParseDeindex()
  {
    var expected = LHE("value", Deindex(NumLit(0)));
    var actual = SolParser.Parse("value[0]");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocation()
  {
    var expected = LHE("value", Invoke());
    var actual = SolParser.Parse("value()");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithArg()
  {
    var expected = LHE("value", Invoke(LHE("a")));
    var actual = SolParser.Parse("value(a)");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithMultipleArg()
  {
    var expected = LHE("value", Invoke(LHE("a"), LHE("b")));
    var actual = SolParser.Parse("value(a, b)");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithDeref()
  {
    var expected = LHE("value", Invoke(), Deref("field"));
    var actual = SolParser.Parse("value().field");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseInvocationWithDeindex()
  {
    var expected = LHE("value", Invoke(), Deindex(NumLit(0)));
    var actual = SolParser.Parse("value()[0]");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeindexWithInvocation()
  {
    var expected = LHE("value", Deindex(NumLit(0)), Invoke());
    var actual = SolParser.Parse("value[0]()");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDeindexWithDeref()
  {
    var expected = LHE("value", Deindex(NumLit(0)), Deref("field"));
    var actual = SolParser.Parse("value[0].field");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDerefWithInvocation()
  {
    var expected = LHE("value", Deref("field"), Invoke());
    var actual = SolParser.Parse("value.field()");
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
          list.Add((Deindex(NumLit(index)), $"[{index}]"));
          break;
      }
    }
    var source = $"value{list.Select(x => x.source).StringJoin()}";
    var actual = SolParser.Parse(source);
    var expected = LHE("value", list.Select(x => x.func).ToArray());
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseBooleanNegate()
  {
    var expected = Unary("!", LHE("a"));
    var actual = SolParser.Parse("!a");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseRealNegate()
  {
    var expected = Unary("-", LHE("a"));
    var actual = SolParser.Parse("-a");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAdd()
  {
    var expected = Binary("+", LHE("a"), LHE("b"));
    var actual = SolParser.Parse("a + b");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseSub()
  {
    var expected = Binary("-", LHE("a"), LHE("b"));
    var actual = SolParser.Parse("a - b");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDoubleSub()
  {
    var expected = Binary("-", LHE("a"), Unary("-", LHE("b")));
    var actual = SolParser.Parse("a - - b");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMul()
  {
    var expected = Binary("*", LHE("a"), LHE("b"));
    var actual = SolParser.Parse("a * b");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseDiv()
  {
    var expected = Binary("/", LHE("a"), LHE("b"));
    var actual = SolParser.Parse("a / b");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseMod()
  {
    var expected = Binary("%", LHE("a"), LHE("b"));
    var actual = SolParser.Parse("a % b");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void OpsAreOrderedCorrectly()
  {
    var expected = Binary("+", Binary("*", LHE("a"), LHE("b")), Binary("/", LHE("c"), LHE("d")));
    var actual = SolParser.Parse("a * b + c / d");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseLeadingMinus()
  {
    var expected = Binary(
      "+",
      Binary("*", Unary("-", LHE("a")), LHE("b")),
      Binary("/", LHE("c"), LHE("d"))
    );
    var actual = SolParser.Parse("-a * b + c / d");
    AssertASTCompare(actual, expected);
  }

  [Test]
  public void CanParseAssign()
  {
    var expected = Assign(LHE("a"), LHE("b"));
    var actual = SolParser.Parse("a = b");
    AssertASTCompare(actual, expected);
  }
}
