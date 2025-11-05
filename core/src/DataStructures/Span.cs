using CriusNyx.Util;
using Superpower.Model;

namespace Sol.DataStructures;

public struct Span
{
  public int start;
  public int length;
  public int End => start + length;

  public Span(int start, int length)
  {
    this.start = start;
    this.length = length;
  }

  public static Span Join(params Span[] spans)
  {
    var min = spans.Min(x => x.start);
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

  public static Span Empty => new Span { start = 0, length = 0 };

  public static Span operator -(Span span, int start)
  {
    if (start > span.start)
    {
      throw new InvalidOperationException("Start must be less then span");
    }
    return new Span(start, span.start - start);
  }
}

public static class SpanExtensions
{
  public static string Substring(this string src, Span span)
  {
    return src.Substring(span.start, span.length);
  }
}
