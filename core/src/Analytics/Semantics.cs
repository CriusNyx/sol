using CriusNyx.Util;
using Sol.DataStructures;

namespace Sol;

public enum SemanticType
{
  None,
  Keyword,
  ClassName,
  ObjectReference,
  MethodReference,
  StringLit,
  NumLit,
}

public class SemanticToken(Span span, SemanticType type)
{
  public Span Span => span;
  public SemanticType Type => type;
}

public static class SemanticsAnalysis
{
  public static IEnumerable<(string source, SemanticToken token)> Stream(
    this IEnumerable<SemanticToken> list,
    string source
  )
  {
    int current = 0;
    foreach (var element in list)
    {
      if (element.Span.Start > current)
      {
        var delta = element.Span - current;
        yield return source.Substring(delta).With(new SemanticToken(delta, SemanticType.None));

        current = element.Span.Start;
      }
      {
        yield return source.Substring(element.Span).With(element);
        current = element.Span.End;
      }
    }

    if (current != source.Length)
    {
      var delta = new Span(current, source.Length - current);
      yield return source.Substring(delta).With(new SemanticToken(delta, SemanticType.None));
    }
  }
}
