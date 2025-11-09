using CriusNyx.Util;
using Superpower.Model;

namespace Sol.DataStructures;

public class Span
{
  public int Start { get; private set; }
  public int Length { get; private set; }
  public int End => Start + Length;

  public Span(int start, int length)
  {
    this.Start = start;
    this.Length = length;
  }

  public static Span Join(params Span[] spans)
  {
    var min = spans.Min(x => x.Start);
    var max = spans.Max(x => x.End);
    return new Span(min, max - min);
  }

  public static Span SafeJoin(params Span?[] spans)
  {
    return Join(spans.WhereAs<Span>().ToArray());
  }

  public static implicit operator Span(TextSpan source)
  {
    return new Span(source.Position.Absolute, source.Length);
  }

  public static Span Empty => new Span(0, 0);

  public static Span operator -(Span span, int start)
  {
    if (start > span.Start)
    {
      throw new InvalidOperationException("Start must be less then span");
    }
    return new Span(start, span.Start - start);
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
}

public static class SpanExtensions
{
  public static string Substring(this string src, Span span)
  {
    return src.Substring(span.Start, span.Length);
  }
}
