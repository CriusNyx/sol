using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

public class DeindexExpression(
  SourceSpan? leftBracket,
  RightHandExpression? index,
  SourceSpan? rightBracket,
  LeftHandExpressionChain? chain
) : LeftHandExpressionChain
{
  public SourceSpan? LeftBracket => leftBracket;
  public RightHandExpression? Index => index;
  public SourceSpan? RightBracket => rightBracket;
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

  public override Span GetSpan()
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
