using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

public abstract partial class ASTNode : DebugPrint
{
  protected DevConType? cachedType;
  public DevConType NodeType => cachedType.NotNull();
  public DevConType? NodeTypeSafe => cachedType;
  public abstract IEnumerable<(string, object)> EnumerateFields();

  public DevConType? TypeCheck(TypeContext context)
  {
    cachedType = _TypeCheck(context);
    return cachedType;
  }

  protected abstract DevConType? _TypeCheck(TypeContext context);

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

  public string Dbg()
  {
    return this.Debug();
  }

  public virtual ASTNode? GetNodeUnderCursor(int position)
  {
    if (GetSpan().Contains(position, true))
    {
      foreach (var child in GetChildren())
      {
        if (child.GetNodeUnderCursor(position) is ASTNode node)
        {
          return node;
        }
      }
      return this;
    }
    return null!;
  }
}
