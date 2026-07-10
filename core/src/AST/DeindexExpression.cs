using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for a deindex expression.
/// </summary>
/// <param name="leftBracket"></param>
/// <param name="index"></param>
/// <param name="rightBracket"></param>
/// <param name="chain"></param>
public class DeindexExpression(
  SourceSpan? leftBracket,
  RightHandExpression? index,
  SourceSpan? rightBracket,
  LeftHandExpressionChain? chain
) : LeftHandExpressionChain
{
  /// <summary>
  /// The SourceSpan for the left bracket.
  /// </summary>
  public SourceSpan? LeftBracket => leftBracket;

  /// <summary>
  /// The expression used to compute the index.
  /// </summary>
  public RightHandExpression? Index => index;

  /// <summary>
  /// The SourceSpan for the right bracked.
  /// </summary>
  public SourceSpan? RightBracket => rightBracket;

  /// <summary>
  /// The Chain expression for the next part of the LeftHandExpression.
  /// </summary>
  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Index).With(index)!, nameof(Chain).With(chain)!];
  }

  public override object Evaluate(object underlying, ExecutionContext context)
  {
    dynamic dyn = underlying;
    var index = Evaluate(Index.NotNull(), context);
    return dyn[index];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    throw new NotImplementedException();
  }

  protected override Span _GetSpan()
  {
    return Span.SafeJoin(
      LeftBracket?.GetSpan(),
      Index?.GetSpan(),
      RightBracket?.GetSpan(),
      Chain?.GetSpan()
    );
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { LeftBracket, Index, RightBracket, Chain }.WhereAs<ASTNode>();
  }
}
