using CriusNyx.Util;
using Superpower;
using Superpower.Model;

namespace Sol.Parser;

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
    TextParser<U> separator,
    Func<TextParser<T>, TextParser<T>>? recoveryStrategy = null
  )
  {
    return parser
      .Select(x => new List<T>() { x })
      .ThenChain(
        separator,
        (recoveryStrategy?.Invoke(parser) ?? parser).Select(x => new List<T>() { x }),
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

  public static TextParser<T> RecoverWith<T, R>(
    this TextParser<T> original,
    TextParser<R> recoveryParser,
    Func<R, Result<T>, T> errorTransformer
  )
  {
    return delegate(TextSpan i)
    {
      var result = original(i);
      if (result.HasValue)
      {
        return result;
      }
      else
      {
        return recoveryParser.Select(recovery => errorTransformer(recovery, result))(i);
      }
    };
  }

  public static TextParser<(T, U)> ThenWith<T, U>(this TextParser<T> parser, TextParser<U> then)
  {
    return parser.Then((prev) => then.Select(next => prev.With(next)));
  }

  public static TextParser<(T, U, V)> AndThenWith<T, U, V>(
    this TextParser<(T, U)> parser,
    TextParser<V> then
  )
  {
    return parser.Then((prev) => then.Select(next => prev.AndWith(next)));
  }

  public static TextParser<(T value, U context)> WithContext<T, U>(
    this TextParser<T> parser,
    U context
  )
  {
    return parser.Select(result => result.With(context));
  }

  public static TextParser<T> ThenChain<T, U>(
    this TextParser<T> first,
    TextParser<U> op,
    TextParser<T> rest,
    Func<U, T, T, T> combine
  )
  {
    return Parse.OneOf(
      from f in first
      from r in op.ThenWith(rest).Many()
      select r.Aggregate(f, (a, b) => combine(b.Item1, a, b.Item2)),
      first
    );
  }
}
