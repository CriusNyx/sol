using CriusNyx.Util;
using Sol.DataStructures;
using Sol.TypeSystem;
using ExecutionContext = Sol.Execution.ExecutionContext;

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

  protected override SolType? _TypeCheck(TypeContext context)
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

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(GetSpan(), NodeType.ToSemanticType())];
  }
}
