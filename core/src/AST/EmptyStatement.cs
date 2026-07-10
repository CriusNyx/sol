using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for an empty statement.
/// </summary>
/// <param name="span"></param>
public class EmptyStatement(Span span) : ASTNode
{
  /// <summary>
  /// The span for this empty statement.
  /// </summary>
  public Span Span => span;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    return null;
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [];
  }

  protected override Span _GetSpan() => Span;

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    return new VoidType();
  }
}
