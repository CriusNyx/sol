using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

public class Identifier(SourceSpan? textSpan) : ASTNode
{
  public SourceSpan? Span => textSpan;
  public string Source => Span?.Source.ToString() ?? "";

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Span).With(Span)!];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    throw new InvalidOperationException();
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    throw new NotImplementedException();
  }

  public override Span GetSpan()
  {
    return Span?.GetSpan() ?? DataStructures.Span.Empty;
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { Span }.WhereAs<ASTNode>();
  }

  public void SetType(DevConType devConType)
  {
    this.cachedType = devConType;
  }

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(GetSpan(), NodeTypeSafe?.ToSemanticType() ?? SemanticType.ObjectReference)];
  }
}
