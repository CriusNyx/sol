using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for a number literal.
/// </summary>
/// <param name="source"></param>
/// <param name="value"></param>
public class NumberLiteralExpression(SourceSpan source, NumVal value) : RightHandExpression
{
  /// <summary>
  /// The SourceSpan for the number literal.
  /// </summary>
  public SourceSpan Source => source;

  /// <summary>
  /// The value of the number literal.
  /// </summary>
  public NumVal Value => value;

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
    return new CSType(typeof(NumVal));
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
    return [new(_GetSpan(), SemanticType.NumLit)];
  }
}
