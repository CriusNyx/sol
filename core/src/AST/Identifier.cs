using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for identifiers.
/// </summary>
/// <param name="textSpan"></param>
public class Identifier(SourceSpan? textSpan) : ASTNode
{
  /// <summary>
  /// The SourceSpan for the identifier text.
  /// </summary>
  public SourceSpan? Span => textSpan;

  /// <summary>
  /// The source code for the identifier.
  /// This is the same as the identifier as a string.
  /// </summary>
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

  protected override Span _GetSpan()
  {
    return Span?.GetSpan() ?? DataStructures.Span.Empty;
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { Span }.WhereAs<ASTNode>();
  }

  /// <summary>
  /// Set the type of the identifier.
  /// </summary>
  /// <param name="devConType"></param>
  public void SetType(DevConType devConType)
  {
    cachedType.Insert(devConType);
  }

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(_GetSpan(), NodeTypeSafe?.ToSemanticType() ?? SemanticType.ObjectReference)];
  }
}
