using CriusNyx.Util;

namespace Sol.AST;

public abstract partial class ASTNode : DebugPrint
{
  protected SolType? cachedType;
  public SolType NodeType => cachedType.NotNull();
  public abstract IEnumerable<(string, object)> EnumerateFields();

  public SolType? TypeCheck(TypeCheckerContext context)
  {
    cachedType = _TypeCheck(context);
    return cachedType;
  }

  protected abstract SolType? _TypeCheck(TypeCheckerContext context);

  public abstract object? Evaluate(ExecutionContext context);

  public abstract Span GetSpan();

  public abstract IEnumerable<ASTNode> GetChildren();
}
