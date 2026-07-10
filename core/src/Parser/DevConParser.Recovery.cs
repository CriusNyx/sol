using System.Data;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using SParse = Superpower.Parse;

namespace DevCon.Parser;

public static partial class DevConParser
{
  /// <summary>
  /// Helper method to recover with an empty string.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="source"></param>
  /// <returns></returns>
  public static TextParser<(T, ParseContext)> WithEmptyContext<T>(this TextParser<T> source)
  {
    return source.WithContext(new ParseContext());
  }

  /// <summary>
  /// Helper to recover with an empty string.
  /// </summary>
  /// <param name="parser"></param>
  /// <returns></returns>
  public static TextParser<(TextSpan value, ParseContext context)> RecoverEmptyWithContext(
    this TextParser<(TextSpan, ParseContext)> parser
  )
  {
    return parser.RecoverWith(
      SParse.Return<object?>(null),
      (_, e) => (TextSpan.Empty, new ParseContext(ParseError.From(e)))
    );
  }

  /// <summary>
  /// Helper to recover with a null value.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="parser"></param>
  /// <returns></returns>
  public static TextParser<(T value, ParseContext context)> RecoverNullWithContext<T>(
    this TextParser<(T, ParseContext)> parser
  )
    where T : class
  {
    return parser.RecoverWith(
      SParse.Return<object?>(null),
      (_, e) => (null, new ParseContext(ParseError.From(e)))!
    );
  }

  /// <summary>
  /// Helper to recover with the specified value.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="parser"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public static TextParser<(T value, ParseContext context)> RecoverValueWithContext<T>(
    this TextParser<(T, ParseContext)> parser,
    T value
  )
    where T : class
  {
    return parser.RecoverWith(
      SParse.Return<object?>(null),
      (_, e) => (value, new ParseContext(ParseError.From(e)))!
    );
  }

  /// <summary>
  /// Helper method to recover when parsing.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="parser"></param>
  /// <param name="recoveryParser"></param>
  /// <returns></returns>
  public static TextParser<(T value, ParseContext context)> RecoverWithContext<T>(
    this TextParser<(T, ParseContext)> parser,
    TextParser<T> recoveryParser
  )
  {
    return parser.RecoverWith(
      recoveryParser,
      (value, e) => (value, new ParseContext(ParseError.From(e)))
    );
  }

  /// <summary>
  /// Helper method to recover until a particular symbol is encounter.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="source"></param>
  /// <param name="until"></param>
  /// <returns></returns>
  public static TextParser<(T value, ParseContext context)> RecoverUntilWithContext<T>(
    this TextParser<(T value, ParseContext context)> source,
    params TextParser<TextSpan>[] until
  )
  {
    return source.RecoverWithContext(RecoverUntil(until).Select(x => default(T))!);
  }

  /// <summary>
  /// Recover until the until parser is satisfied.
  /// </summary>
  /// <param name="until"></param>
  /// <returns></returns>
  public static TextParser<TextSpan> RecoverUntil(params TextParser<TextSpan>[] until)
  {
    return Character
      .AnyChar.ManyDelimitedBy(SParse.Not(SParse.OneOf(until)))
      .WithSpan()
      .Select(x => x.span);
  }
}
