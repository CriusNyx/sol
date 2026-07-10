using CriusNyx.Util;
using DevCon.AST;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using Superpower.Model;
using ExecutionContext = DevCon.Execution.ExecutionContext;

/// <summary>
/// ASTNode for a span of source code with no extra grammar information.
/// For many ASTNodes it will have source spans as leaves.
/// </summary>
/// <param name="span"></param>
/// <param name="source"></param>
public class SourceSpan(Span span, string source) : ASTNode
{
  /// <summary>
  /// The span of the source code.
  /// </summary>
  public Span Span => span;

  /// <summary>
  /// The source code for this span.
  /// </summary>
  public string Source => source;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Source).With(Source)];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    throw new NotImplementedException();
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [];
  }

  protected override Span _GetSpan()
  {
    return span;
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    throw new NotImplementedException();
  }

  public override string ShortCode()
  {
    return Source.ToString();
  }

  /// <summary>
  /// Convert Superpower span to a source span.
  /// </summary>
  /// <param name="textSpan"></param>
  public static implicit operator SourceSpan(TextSpan textSpan)
  {
    return new SourceSpan(
      textSpan,
      textSpan.Source.NotNull().Substring(textSpan.Position.Absolute, textSpan.Length)
    );
  }

  public override ASTNode? GetNodeUnderCursor(int position)
  {
    return null;
  }
}
