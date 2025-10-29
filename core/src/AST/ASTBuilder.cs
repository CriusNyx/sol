using Sol.Parser;
using Superpower;
using Superpower.Model;

namespace Sol.AST;

public static class ASTBuilder
{
  public static Identifier Ident(string ident)
  {
    return new Identifier(new TextSpan(ident));
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Deref(string ident)
  {
    return (chain) => new DerefExpression(Ident(ident), chain);
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Deindex(
    RightHandExpression index
  )
  {
    return (chain) => new DeindexExpression(index, chain);
  }

  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Invoke(
    params RightHandExpression[] args
  )
  {
    return (chain) => new InvocationExpression(args, chain);
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
    return new UnaryOp(SolParser.UnaryOpTypeParser.Parse(op), operand);
  }

  public static BinaryOp Binary(string op, RightHandExpression left, RightHandExpression right)
  {
    return new BinaryOp(
      Parse.OneOf(SolParser.TermOpTypeParser, SolParser.FactorOpTypeParser).Parse(op),
      left,
      right
    );
  }

  public static Assign Assign(LeftHandExpression left, RightHandExpression right)
  {
    return new Assign(left, right);
  }

  public static NumberLiteralExpression NumLit(decimal value)
  {
    return new NumberLiteralExpression(new NumVal(value));
  }

  public static StringLiteralExpression StringLit(string value)
  {
    return new StringLiteralExpression(value);
  }
}
