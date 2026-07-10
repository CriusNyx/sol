using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for a DevCon program.
/// </summary>
/// <param name="statements"></param>
public class DevConProgram(ASTNode[] statements) : ASTNode
{
  /// <summary>
  /// The statements that comprise this program.
  /// </summary>
  public ASTNode?[] Statements => statements;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Statements).With(Statements)];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    object? output = null;
    foreach (var Statement in Statements)
    {
      output = Statement.NotNull().Evaluate(context);
    }
    return output;
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    DevConType? result = null;
    foreach (var statement in Statements)
    {
      result = statement?.TypeCheck(context) ?? new UnknownType(null);
    }
    return result;
  }

  protected override Span _GetSpan()
  {
    return Span.Join(statements.Select(x => x.GetSpan()).ToArray());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return statements;
  }
}
