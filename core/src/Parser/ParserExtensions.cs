using CriusNyx.Util;
using Superpower;
using Superpower.Model;

namespace Sol.Parser.Extensions;

public static class ParserExtensions
{
  public static TextParser<T> ThenIgnore<T, U>(this TextParser<T> source, TextParser<U> ignore)
  {
    return source.Then((content) => ignore.Select((_) => content));
  }

  public static TextParser<T> SurroundedBy<T, U, V>(
    this TextParser<T> content,
    TextParser<U> before,
    TextParser<V> after
  )
  {
    return before.IgnoreThen(content).ThenIgnore(after);
  }

  public static TextParser<T> FullText<T>(this TextParser<T> parser)
  {
    return SolToken.NonSemantic.IgnoreThen(parser).AtEnd();
  }

  public static TextParser<T[]> SeparatedBy<T, U>(
    this TextParser<T> parser,
    TextParser<U> separator
  )
  {
    return Parse
      .Chain(
        separator,
        parser.Select(x => new List<T>() { x }),
        (_, l, r) => l.Touch(x => x.AddRange(r))
      )
      .Select(x => x.ToArray());
  }

  public static TextParser<(TextSpan span, T value)> WithSpan<T>(this TextParser<T> parser)
  {
    return delegate(TextSpan i)
    {
      Result<T> result = parser(i);
      return (!result.HasValue)
        ? Result.CastEmpty<T, (TextSpan, T)>(result)
        : Result.Value(i.Until(result.Remainder).With(result.Value), i, result.Remainder);
    };
  }
}
