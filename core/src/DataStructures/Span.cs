using CriusNyx.Util;
using Superpower.Model;

namespace DevCon.DataStructures;

public class Span : DebugPrint
{
  public int Start { get; private set; }
  public int Length { get; private set; }
  public int Line { get; private set; }
  public int Column { get; private set; }
  public int End => Start + Length;

  public Span(int start, int length, int line, int column)
  {
    Start = start;
    Length = length;
    Line = line;
    Column = column;
  }

  public static Span Join(params Span[] spans)
  {
    var minSpan = spans.MinBy(x => x.Start);
    var min = spans.Min(x => x.Start);
    var max = spans.Max(x => x.End);
    return new Span(min, max - min, minSpan?.Line ?? 0, minSpan?.Column ?? 0);
  }

  public static Span SafeJoin(params Span?[] spans)
  {
    return Join(spans.WhereAs<Span>().ToArray());
  }

  public static implicit operator Span(TextSpan source)
  {
    return new Span(
      source.Position.Absolute,
      source.Length,
      source.Position.Line,
      source.Position.Column
    );
  }

  public static Span Empty => new Span(0, 0, 0, 0);

  public static Span operator -(Span span, int start)
  {
    if (start > span.Start)
    {
      throw new InvalidOperationException("Start must be less then span");
    }
    return new Span(start, span.Start - start, -1, -1);
  }

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
  public static string Substring(this string src, Span span)
  {
    return src.Substring(span.Start, span.Length);
  }
}
