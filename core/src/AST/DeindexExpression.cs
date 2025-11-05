using CriusNyx.Util;
using Sol.DataStructures;
using Sol.TypeSystem;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol.AST;

public class DeindexExpression(
  SourceSpan leftBracket,
  RightHandExpression index,
  SourceSpan rightBracket,
  LeftHandExpressionChain? chain
) : LeftHandExpressionChain
{
  public SourceSpan LeftBracket => leftBracket;
  public RightHandExpression Index => index;
  public SourceSpan RightBracket => rightBracket;
  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Index).With(index), nameof(Chain).With(chain)!];
  }

  public override object Evaluate(object underlying, ExecutionContext context)
  {
    dynamic dyn = underlying;
    var index = Evaluate(Index, context);
    return dyn[index];
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    throw new NotImplementedException();
  }

  public override Span GetSpan()
  {
    return Span.SafeJoin(
      LeftBracket.GetSpan(),
      index.GetSpan(),
      RightBracket.GetSpan(),
      Chain?.GetSpan()
    );
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    yield return LeftBracket;
    yield return Index;
    yield return RightBracket;
    if (Chain != null)
    {
      yield return Chain;
    }
  }
}
