using CriusNyx.Util;
using DevCon.DataStructures;

namespace DevCon;

/// <summary>
/// The type of the SemanticToken.
/// </summary>
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

/// <summary>
/// A token containing SemanticInformation for the token.
/// </summary>
/// <param name="span"></param>
/// <param name="type"></param>
public class SemanticToken(Span span, SemanticType type) : DebugPrint
{
  /// <summary>
  /// The source span of the semantic token.
  /// </summary>
  public Span Span => span;

  /// <summary>
  /// The type of this token.
  /// </summary>
  public SemanticType Type => type;

  public IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Span).With(Span), nameof(Type).With(Type)];
  }
}

/// <summary>
/// Analyzer for the semantics of a program.
/// </summary>
public static class SemanticsAnalysis
{
  public const string keywordColor = "#569cd6";
  public const string fieldColor = "#9cdcfe";
  public const string classNameColor = "#4ec9b0";
  public const string methodColor = "#dcdcaa";
  public const string stringLitColor = "#ce9178";
  public const string numLitColor = "#b5cea8";

  /// <summary>
  /// Create a stream of semantic tokens with no gaps.
  /// </summary>
  /// <param name="list"></param>
  /// <param name="source"></param>
  /// <returns></returns>
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
        yield return source.SpanSubstring(delta).With(new SemanticToken(delta, SemanticType.None));

        current = element.Span.Start;
      }
      {
        yield return source.SpanSubstring(element.Span).With(element);
        current = element.Span.End;
      }
    }

    if (current != source.Length)
    {
      var delta = new Span(current, source.Length - current, -1, -1);
      yield return source.SpanSubstring(delta).With(new SemanticToken(delta, SemanticType.None));
    }
  }
}
