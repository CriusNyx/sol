using DevCon.Execution;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for a part of a LeftHandExpressionChain.
/// </summary>
public abstract class LeftHandExpressionChain() : ASTNode
{
  public override object? Evaluate(ExecutionContext context)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Evaluate this part of the chain as a reference so that it can be dereferenced or assigned.
  /// </summary>
  /// <param name="underlying"></param>
  /// <param name="context"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  public virtual ObjectReference EvaluateReference(
    ObjectReference underlying,
    ExecutionContext context
  )
  {
    throw new NotImplementedException();
  }

  public abstract object Evaluate(object underlying, ExecutionContext context);
}
