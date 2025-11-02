using CriusNyx.Util;
using Sol.AST;

public class ArgumentInfo(Identifier? explicitParameterName, RightHandExpression expression)
  : ASTNode
{
  public Identifier? ExplicitParameterName => explicitParameterName;
  public RightHandExpression Expression => expression;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return
    [
      nameof(ExplicitParameterName).With(ExplicitParameterName)!,
      nameof(Expression).With(Expression),
    ];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    throw new NotImplementedException();
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    throw new NotImplementedException();
  }

  public override Span GetSpan()
  {
    throw new NotImplementedException();
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    return Expression.TypeCheck(context);
  }
}
