using CriusNyx.Util;
using DevCon;
using DevCon.DataStructures;
using Superpower.Model;

/// <summary>
/// ASTNode for a text span for a keyword.
/// </summary>
/// <param name="span"></param>
/// <param name="source"></param>
public class KeywordSpan(Span span, string source) : SourceSpan(span, source)
{
  public KeywordSpan(TextSpan textSpan)
    : this(
      textSpan,
      textSpan.Source.NotNull().Substring(textSpan.Position.Absolute, textSpan.Length)
    ) { }

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(_GetSpan(), SemanticType.Keyword)];
  }
}
