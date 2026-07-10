using CriusNyx.Util;
using DevCon;
using DevCon.AST;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

/// <summary>
/// ASTNode for a string literal expression.
/// </summary>
/// <param name="source"></param>
/// <param name="value"></param>
public class StringLiteralExpression(SourceSpan source, string value) : RightHandExpression
{
  /// <summary>
  /// SourceSpan for the whole literal, including the opening and closing paren.
  /// </summary>
  public SourceSpan Source => source;

  /// <summary>
  /// The string value of the literal.
  /// </summary>
  public string Value => value;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Value).With(Value)];
  }

  public override object Evaluate(ExecutionContext context)
  {
    return Value;
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    return new CSType(typeof(string));
  }

  protected override Span _GetSpan()
  {
    return Source.GetSpan();
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [Source];
  }

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(_GetSpan(), SemanticType.StringLit)];
  }
}
