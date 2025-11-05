using CriusNyx.Util;
using Sol.DataStructures;
using Sol.TypeSystem;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol.AST;

public abstract partial class ASTNode : DebugPrint
{
  protected SolType? cachedType;
  public SolType NodeType => cachedType.NotNull();
  public SolType? NodeTypeSafe => cachedType;
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

  public virtual IEnumerable<SemanticToken> GetSemantics()
  {
    return GetChildren().WhereAs<ASTNode>().SelectMany(x => x?.GetSemantics() ?? []);
  }

  public virtual string ShortCode()
  {
    return "";
  }
}
