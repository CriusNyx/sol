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
    return new Identifier(new(new(ident)));
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Deref(string ident)
  {
    return (chain) => new DerefExpression(new(new(".")), Ident(ident), chain);
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Deindex(
    RightHandExpression index
  )
  {
    return (chain) => new DeindexExpression(new(new("[")), index, new(new("]")), chain);
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Invoke(
    params RightHandExpression[] args
  )
  {
    return (chain) => new InvocationExpression(new(new("(")), args, new(new(")")), chain);
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
      new(new(op)),
      SolParser.UnaryOpTypeParser.Select(x => x.value).Parse(op),
      operand
    );
  }

  public static BinaryOp Binary(string op, RightHandExpression left, RightHandExpression right)
  {
    return new BinaryOp(
      new(new(op)),
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
    return new Assign(left, new(new("=")), right);
  }

  public static UseStatement Use(params Identifier[] identifiers)
  {
    return new UseStatement(new(new("use")), identifiers);
  }

  public static UseStatement UseExplicit(Identifier[] identifiers)
  {
    return new UseStatement(new(new("use")), identifiers);
  }

  public static NumberLiteralExpression NumLit(string source)
  {
    return new NumberLiteralExpression(new(new(source)), new NumVal(decimal.Parse(source)));
  }

  public static StringLiteralExpression StringLit(string value)
  {
    return new StringLiteralExpression(new(new(value)), value);
  }

  public static SolProgram Prog(params ASTNode[] nodes)
  {
    return new SolProgram(nodes);
  }
}
