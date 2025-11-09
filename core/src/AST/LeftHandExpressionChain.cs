using DevCon.Execution;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

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
