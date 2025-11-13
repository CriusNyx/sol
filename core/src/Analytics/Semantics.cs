using CriusNyx.Util;
using DevCon.DataStructures;

namespace DevCon;

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

public class SemanticToken(Span span, SemanticType type) : DebugPrint
{
  public Span Span => span;
  public SemanticType Type => type;

  public IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Span).With(Span), nameof(Type).With(Type)];
  }
}

public static class SemanticsAnalysis
{
  public const string keywordColor = "#569cd6";
  public const string fieldColor = "#9cdcfe";
  public const string classNameColor = "#4ec9b0";
  public const string methodColor = "#dcdcaa";
  public const string stringLitColor = "#ce9178";
  public const string numLitColor = "#b5cea8";

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
      var delta = new Span(current, source.Length - current, -1, -1);
      yield return source.Substring(delta).With(new SemanticToken(delta, SemanticType.None));
    }
  }
}
