using CriusNyx.Util;
using Superpower;
using Superpower.Model;

namespace DevCon.Parser;

/// <summary>
/// Helper methods to modify parsers.
/// </summary>
public static class ParserExtensions
{
  /// <summary>
  /// Execute the source parser, then the ignore parser, returning the results of the source parser only.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <param name="source"></param>
  /// <param name="ignore"></param>
  /// <returns></returns>
  public static TextParser<T> ThenIgnore<T, U>(this TextParser<T> source, TextParser<U> ignore)
  {
    return source.Then((content) => ignore.Select((_) => content));
  }

  /// <summary>
  /// Succeeds with the source parser is surrounded by the before and after parser.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <typeparam name="V"></typeparam>
  /// <param name="content"></param>
  /// <param name="before"></param>
  /// <param name="after"></param>
  /// <returns></returns>
  public static TextParser<T> SurroundedBy<T, U, V>(
    this TextParser<T> content,
    TextParser<U> before,
    TextParser<V> after
  )
  {
    return before.IgnoreThen(content).ThenIgnore(after);
  }

  /// <summary>
  /// Parser followed by end of input.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="source"></param>
  /// <returns></returns>
  public static TextParser<T> FullText<T>(this TextParser<T> source)
  {
    return DevConToken.NonSemantic.IgnoreThen(source).AtEnd();
  }

  /// <summary>
  /// Return a sequence of elements parsed by source separated by separator parser.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <param name="source"></param>
  /// <param name="separator"></param>
  /// <param name="recoveryStrategy"></param>
  /// <returns></returns>
  public static TextParser<T[]> SeparatedBy<T, U>(
    this TextParser<T> source,
    TextParser<U> separator,
    Func<TextParser<T>, TextParser<T>>? recoveryStrategy = null
  )
  {
    return source
      .Select(x => new List<T>() { x })
      .ThenChain(
        separator,
        (recoveryStrategy?.Invoke(source) ?? source).Select(x => new List<T>() { x }),
        (_, l, r) => l.Touch(x => x.AddRange(r))
      )
      .Select(x => x.ToArray());
  }

  /// <summary>
  /// Return the result of the parser with the source span.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="parser"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Recover with the specified parser and return the result.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="R"></typeparam>
  /// <param name="original"></param>
  /// <param name="recoveryParser"></param>
  /// <param name="errorTransformer"></param>
  /// <returns></returns>
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

  /// <summary>
  /// Create a new parser which is the source parser followed by the then parser.
  /// Return the results as a tuple.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <param name="source"></param>
  /// <param name="then"></param>
  /// <returns></returns>
  public static TextParser<(T, U)> ThenWith<T, U>(this TextParser<T> source, TextParser<U> then)
  {
    return source.Then((prev) => then.Select(next => prev.With(next)));
  }

  /// <summary>
  /// Create a new parser which is the source parser followed by the then parser.
  /// Return the results as a tuple.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <typeparam name="V"></typeparam>
  /// <param name="source"></param>
  /// <param name="then"></param>
  /// <returns></returns>
  public static TextParser<(T, U, V)> AndThenWith<T, U, V>(
    this TextParser<(T, U)> source,
    TextParser<V> then
  )
  {
    return source.Then((prev) => then.Select(next => prev.AndWith(next)));
  }

  /// <summary>
  /// Return the result of source with the context appended as a tuple.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <param name="source"></param>
  /// <param name="context"></param>
  /// <returns></returns>
  public static TextParser<(T value, U context)> WithContext<T, U>(
    this TextParser<T> source,
    U context
  )
  {
    return source.Select(result => result.With(context));
  }

  /// <summary>
  /// Extension of Superpower Chain parser which suppors recovery.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <param name="first"></param>
  /// <param name="op"></param>
  /// <param name="rest"></param>
  /// <param name="combine"></param>
  /// <returns></returns>
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
