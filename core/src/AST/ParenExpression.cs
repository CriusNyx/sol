using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for a paren expression.
/// In the gramar parens function to convert any right hand expression into a unit expression.
/// </summary>
/// <param name="leftParen"></param>
/// <param name="rightHandExpression"></param>
/// <param name="rightParen"></param>
public class ParenExpression(
  SourceSpan? leftParen,
  RightHandExpression? rightHandExpression,
  SourceSpan? rightParen
) : RightHandExpression
{
  /// <summary>
  /// The SourceSpan for the left paren.
  /// </summary>
  public SourceSpan? LeftParen => leftParen;

  /// <summary>
  /// The expression inside of the parens.
  /// </summary>
  public RightHandExpression? RightHandExpression => rightHandExpression;

  /// <summary>
  /// The SourceSpan for the right paren.
  /// </summary>
  public SourceSpan? RightParen => rightParen;

  public override object? Evaluate(ExecutionContext context)
  {
    return RightHandExpression.NotNull().Evaluate(context);
  }

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(RightHandExpression).With(rightHandExpression)!];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    return RightHandExpression?.TypeCheck(context) ?? new UnknownType(null);
  }

  protected override Span _GetSpan()
  {
    return Span.SafeJoin(
      LeftParen?.GetSpan(),
      RightHandExpression?.GetSpan(),
      RightParen?.GetSpan()
    );
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { LeftParen, RightHandExpression, RightParen }.WhereAs<ASTNode>();
  }
}
