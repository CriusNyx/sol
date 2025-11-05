using Sol.Execution;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol.AST;

public abstract class LeftHandExpressionChain() : ASTNode
{
  public override object? Evaluate(ExecutionContext context)
  {
    throw new NotImplementedException();
  }

  public virtual ObjectReference EvaluateReference(
    ObjectReference underlying,
    ExecutionContext context
  )
  {
    throw new NotImplementedException();
  }

  public abstract object Evaluate(object underlying, ExecutionContext context);
}
