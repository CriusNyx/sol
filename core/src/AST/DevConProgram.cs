using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

public class DevConProgram(ASTNode[] statements) : ASTNode
{
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
      result = statement?.TypeCheck(context) ?? new UnknownType();
    }
    return result;
  }

  public override Span GetSpan()
  {
    return Span.Join(statements.Select(x => x.GetSpan()).ToArray());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return statements;
  }
}
