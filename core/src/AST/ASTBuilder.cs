using DevCon.DataStructures;
using DevCon.Parser;
using Superpower;

namespace DevCon.AST;

/// <summary>
/// Helper class to build AST's
/// </summary>
public static class ASTBuilder
{
  /// <summary>
  /// Create an IdentifierNode
  /// </summary>
  /// <param name="ident"></param>
  /// <returns></returns>
  public static Identifier Ident(string ident)
  {
    if (ident == null)
    {
      return null!;
    }
    return new Identifier(new(Span.Empty, ident));
  }

  /// <summary>
  /// Create a Deref node.
  /// </summary>
  /// <param name="ident"></param>
  /// <returns></returns>
  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Deref(string ident)
  {
    return (chain) => new DerefExpression(new(Span.Empty, "."), Ident(ident), chain);
  }

  /// <summary>
  /// Create a Deindex node.
  /// </summary>
  /// <param name="index"></param>
  /// <param name="leftBracket"></param>
  /// <param name="rightBracket"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Append an Invocation node and append it to the the chain expression.
  /// </summary>
  /// <param name="args"></param>
  /// <returns></returns>
  public static Func<LeftHandExpressionChain?, LeftHandExpressionChain> Invoke(
    params RightHandExpression[] args
  )
  {
    return Invoke("(", ")", args);
  }

  /// <summary>
  /// Append an Invocation node and append it to the the chain expression.
  /// </summary>
  /// <param name="leftParen"></param>
  /// <param name="rightParen"></param>
  /// <param name="args"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Create a LeftHandExpression.
  /// </summary>
  /// <param name="ident"></param>
  /// <param name="chain"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Create a UnaryOperation.
  /// </summary>
  /// <param name="op"></param>
  /// <param name="operand"></param>
  /// <returns></returns>
  public static UnaryOp Unary(string op, RightHandExpression operand)
  {
    return new UnaryOp(
      new(Span.Empty, op),
      DevConParser.UnaryOpTypeParser.Select(x => x.value).Parse(op),
      operand
    );
  }

  /// <summary>
  /// Create a BinaryOperation.
  /// </summary>
  /// <param name="op"></param>
  /// <param name="left"></param>
  /// <param name="right"></param>
  /// <returns></returns>
  public static BinaryOp Binary(string op, RightHandExpression left, RightHandExpression right)
  {
    return new BinaryOp(
      new(Span.Empty, op),
      Parse
        .OneOf(DevConParser.TermOpTypeParser, DevConParser.FactorOpTypeParser)
        .Select(x => x.value)
        .Parse(op),
      left,
      right
    );
  }

  /// <summary>
  /// Create an Assign statement.
  /// </summary>
  /// <param name="left"></param>
  /// <param name="right"></param>
  /// <returns></returns>
  public static Assign Assign(LeftHandExpression left, RightHandExpression right)
  {
    return new Assign(left, new(Span.Empty, "="), right);
  }

  /// <summary>
  /// Create a Use statement.
  /// </summary>
  /// <param name="identifiers"></param>
  /// <returns></returns>
  public static UseStatement Use(params Identifier[] identifiers)
  {
    return new UseStatement(new(Span.Empty, "use"), identifiers);
  }

  /// <summary>
  /// Create a use statement an explicitly define the path.
  /// </summary>
  /// <param name="identifiers"></param>
  /// <returns></returns>
  public static UseStatement UseExplicit(Identifier[] identifiers)
  {
    return new UseStatement(new(Span.Empty, "use"), identifiers);
  }

  /// <summary>
  /// Create a empty statement.
  /// </summary>
  /// <returns></returns>
  public static EmptyStatement Empty()
  {
    return new EmptyStatement(Span.Empty);
  }

  /// <summary>
  /// Create a number literal.
  /// </summary>
  /// <param name="source"></param>
  /// <returns></returns>
  public static NumberLiteralExpression NumLit(string source)
  {
    return new NumberLiteralExpression(new(Span.Empty, source), new NumVal(decimal.Parse(source)));
  }

  /// <summary>
  /// Create a string literal.
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  public static StringLiteralExpression StringLit(string value)
  {
    return new StringLiteralExpression(new(Span.Empty, value), value);
  }

  /// <summary>
  /// Create a DevConProgram.
  /// </summary>
  /// <param name="nodes"></param>
  /// <returns></returns>
  public static DevConProgram Prog(params ASTNode[] nodes)
  {
    return new DevConProgram(nodes);
  }
}
