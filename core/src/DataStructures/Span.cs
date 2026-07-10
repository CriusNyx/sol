using CriusNyx.Util;
using Superpower.Model;

namespace DevCon.DataStructures;

/// <summary>
/// The span of the source code.
/// </summary>
public class Span : DebugPrint
{
  /// <summary>
  /// The absolute index of the start of the span.
  /// </summary>
  public int Start { get; private set; }

  /// <summary>
  /// The length of the span.
  /// </summary>
  public int Length { get; private set; }

  /// <summary>
  /// The line where the span starts.
  /// </summary>
  public int Line { get; private set; }

  /// <summary>
  /// The column where the span starts.
  /// </summary>
  public int Column { get; private set; }

  /// <summary>
  /// The absolute index for the end of the span.
  /// </summary>
  public int End => Start + Length;

  /// <summary>
  /// Create a new span with the specified parameters.
  /// </summary>
  /// <param name="start"></param>
  /// <param name="length"></param>
  /// <param name="line"></param>
  /// <param name="column"></param>
  public Span(int start, int length, int line, int column)
  {
    Start = start;
    Length = length;
    Line = line;
    Column = column;
  }

  /// <summary>
  /// Join multiple spans together, or return an empty span.
  /// </summary>
  /// <param name="spans"></param>
  /// <returns></returns>
  public static Span Join(params Span[] spans)
  {
    if (spans.Length == 0)
    {
      return Empty;
    }
    var minSpan = spans.MinBy(x => x.Start);
    var min = spans.Min(x => x.Start);
    var max = spans.Max(x => x.End);
    return new Span(min, max - min, minSpan?.Line ?? 0, minSpan?.Column ?? 0);
  }

  /// <summary>
  /// Join multiple spans together. Null spans are ignored.
  /// </summary>
  /// <param name="spans"></param>
  /// <returns></returns>
  public static Span SafeJoin(params Span?[] spans)
  {
    return Join(spans.WhereAs<Span>().ToArray());
  }

  /// <summary>
  /// Covnert Superpower TextSpan to Span.
  /// </summary>
  /// <param name="source"></param>
  public static implicit operator Span(TextSpan source)
  {
    return new Span(
      source.Position.Absolute,
      source.Length,
      source.Position.Line,
      source.Position.Column
    );
  }

  /// <summary>
  /// Create an empty span.
  /// </summary>
  public static Span Empty => new Span(0, 0, 0, 0);

  /// <summary>
  /// Compute the difference between two spans.
  /// </summary>
  /// <param name="span"></param>
  /// <param name="start"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static Span operator -(Span span, int start)
  {
    if (start > span.Start)
    {
      throw new InvalidOperationException("Start must be less then span");
    }
    return new Span(start, span.Start - start, -1, -1);
  }

  /// <summary>
  /// Returns true if this span includes the absolute index.
  /// </summary>
  /// <param name="position"></param>
  /// <param name="inclusive"></param>
  /// <returns></returns>
  public bool Contains(int position, bool inclusive)
  {
    if (inclusive)
    {
      return position >= Start && position <= End;
    }
    else
    {
      return position >= Start && position < End;
    }
  }

  public IEnumerable<(string, object)> EnumerateFields()
  {
    return
    [
      nameof(Start).With(Start),
      nameof(End).With(End),
      nameof(Line).With(Line),
      nameof(Column).With(Column),
    ];
  }
}

public static class SpanExtensions
{
  /// <summary>
  /// Extract a substring from the source code for this span.
  /// </summary>
  /// <param name="src"></param>
  /// <param name="span"></param>
  /// <returns></returns>
  public static string SpanSubstring(this string src, Span span)
  {
    return src.Substring(span.Start, span.Length);
  }
}
