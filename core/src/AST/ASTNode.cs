using CriusNyx.Util;

namespace Sol.AST;

public abstract class ASTNode : DebugPrint
{
  public abstract IEnumerable<(string, object)> EnumerateFields();

  public abstract SolType? TypeCheck(TypeCheckerContext context);

  public abstract object? Evaluate(ExecutionContext context);
}
