using CriusNyx.Util;

namespace Sol.AST;

public class Identifier(SourceSpan textSpan) : ASTNode
{
  public SourceSpan Span => textSpan;
  public string Source => Span.Source.ToString();

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Span).With(Span)];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    throw new InvalidOperationException();
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    throw new NotImplementedException();
  }

  public override Span GetSpan()
  {
    return Span.GetSpan();
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [Span];
  }

  public void SetType(SolType solType)
  {
    this.cachedType = solType;
  }
}
