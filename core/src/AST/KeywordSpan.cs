using CriusNyx.Util;
using DevCon;
using DevCon.DataStructures;
using Superpower.Model;

public class KeywordSpan(Span span, string source) : SourceSpan(span, source)
{
  public KeywordSpan(TextSpan textSpan)
    : this(
      textSpan,
      textSpan.Source.NotNull().Substring(textSpan.Position.Absolute, textSpan.Length)
    ) { }

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(GetSpan(), SemanticType.Keyword)];
  }
}
