using Sol.AST;

public class EmptyStatement(Span span) : ASTNode
{
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

  public override Span GetSpan() => Span;

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    return new VoidType();
  }
}
