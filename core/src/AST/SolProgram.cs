using CriusNyx.Util;

namespace Sol.AST;

public class SolProgram(ASTNode[] statements) : ASTNode
{
  public ASTNode[] Statements => statements;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Statements).With(Statements)];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    object? output = null;
    foreach (var Statement in Statements)
    {
      output = Statement.Evaluate(context);
    }
    return output;
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    SolType? result = null;
    foreach (var statement in Statements)
    {
      result = statement.TypeCheck(context);
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
