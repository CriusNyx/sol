using Sol;
using Superpower.Model;

public class KeywordSpan(TextSpan source) : SourceSpan(source)
{
  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(GetSpan(), SemanticType.Keyword)];
  }
}
