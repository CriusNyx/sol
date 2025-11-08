using Sol.DataStructures;
using Sol.Parser;
using Superpower;

namespace Sol.AST;

public static class ASTBuilder
{
  public static Identifier Ident(string ident)
  {
    if (ident == null)
    {
      return null!;
    }
    return new Identifier(new(Span.Empty, ident));
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Deref(string ident)
  {
    return (chain) => new DerefExpression(new(Span.Empty, "."), Ident(ident), chain);
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Deindex(
    RightHandExpression index,
    string leftBracket = "[",
    string rightBracket = "]"
  )
  {
    return (chain) =>
      new DeindexExpression(
        new(Span.Empty, leftBracket),
        index,
        new(Span.Empty, rightBracket),
        chain
      );
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Invoke(
    params RightHandExpression[] args
  )
  {
    return Invoke("(", ")", args);
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Invoke(
    string leftParen,
    string rightParen,
    params RightHandExpression[] args
  )
  {
    return (chain) =>
      new InvocationExpression(
        new(Span.Empty, leftParen),
        args,
        new(Span.Empty, rightParen),
        chain
      );
  }

  public static LeftHandExpression LHE(
    string ident,
    params Func<LeftHandExpressionChain?, LeftHandExpressionChain>[] chain
  )
  {
    var outChain = chain
      .Reverse()
      .Aggregate(null as LeftHandExpressionChain, (prev, curr) => curr(prev));
    return new LeftHandExpression(Ident(ident), outChain);
  }

  public static UnaryOp Unary(string op, RightHandExpression operand)
  {
    return new UnaryOp(
      new(Span.Empty, op),
      SolParser.UnaryOpTypeParser.Select(x => x.value).Parse(op),
      operand
    );
  }

  public static BinaryOp Binary(string op, RightHandExpression left, RightHandExpression right)
  {
    return new BinaryOp(
      new(Span.Empty, op),
      Parse
        .OneOf(SolParser.TermOpTypeParser, SolParser.FactorOpTypeParser)
        .Select(x => x.value)
        .Parse(op),
      left,
      right
    );
  }

  public static Assign Assign(LeftHandExpression left, RightHandExpression right)
  {
    return new Assign(left, new(Span.Empty, "="), right);
  }

  public static UseStatement Use(params Identifier[] identifiers)
  {
    return new UseStatement(new(Span.Empty, "use"), identifiers);
  }

  public static UseStatement UseExplicit(Identifier[] identifiers)
  {
    return new UseStatement(new(Span.Empty, "use"), identifiers);
  }

  public static NumberLiteralExpression NumLit(string source)
  {
    return new NumberLiteralExpression(new(Span.Empty, source), new NumVal(decimal.Parse(source)));
  }

  public static StringLiteralExpression StringLit(string value)
  {
    return new StringLiteralExpression(new(Span.Empty, value), value);
  }

  public static SolProgram Prog(params ASTNode[] nodes)
  {
    return new SolProgram(nodes);
  }
}
