using System.Data;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using SParse = Superpower.Parse;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<(T, ParseContext)> WithEmptyContext<T>(this TextParser<T> source)
  {
    return source.WithContext(new ParseContext());
  }

  public static TextParser<(TextSpan value, ParseContext context)> RecoverEmptyWithContext(
    this TextParser<(TextSpan, ParseContext)> parser
  )
  {
    return parser.RecoverWith(
      SParse.Return<object?>(null),
      (_, e) => (TextSpan.Empty, new ParseContext(ParseError.From(e)))
    );
  }

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

  public static TextParser<(T value, ParseContext context)> RecoverUntilWithContext<T>(
    this TextParser<(T value, ParseContext context)> source,
    params TextParser<TextSpan>[] until
  )
  {
    return source.RecoverWithContext(RecoverUntil(until).Select(x => default(T))!);
  }

  public static TextParser<TextSpan> RecoverUntil(params TextParser<TextSpan>[] until)
  {
    return Character
      .AnyChar.ManyDelimitedBy(SParse.Not(SParse.OneOf(until)))
      .WithSpan()
      .Select(x => x.span);
  }
}
