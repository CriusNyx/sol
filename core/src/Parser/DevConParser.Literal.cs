using CriusNyx.Util;
using DevCon.AST;
using Superpower;
using SParse = Superpower.Parse;
using SParser = Superpower.Parsers;

namespace DevCon.Parser;

public partial class DevConParser
{
  private static readonly TextParser<char> CStringContentChar = SParser
    .Span.EqualTo("\\\"")
    .Value('"')
    .Try()
    .Or(SParser.Character.ExceptIn('"', '\\', '\r', '\n'));

  // public static TextParser<string> CString { get; } =
  //   SParser
  //     .Character.EqualTo('"')
  //     .IgnoreThen(CStringContentChar.Many())
  //     .Then((char[] s) => SParser.Character.EqualTo('"').Value(new string(s)));

  public static TextParser<(string value, ParseContext context)> CString { get; } =
    from start in SParser.Character.EqualTo('"')
    from body in CStringContentChar
      .Many()
      .Select(x => new string(x))
      .WithEmptyContext()
      .RecoverNullWithContext()
    from end in SParser
      .Character.EqualTo('"')
      .WithEmptyContext()
      .RecoverUntilWithContext(DevConToken.NewLine)
    select body.value.With(ParseContext.Combine(body.context, end.context));

  public static TextParser<(RightHandExpression value, ParseContext context)> NumberLiteralParser =
    SParser
      .Numerics.DecimalDecimal.ThenIgnore(DevConToken.NonSemantic)
      .WithSpan()
      .Select(
        (result) =>
          new NumberLiteralExpression(result.span, new NumVal(result.value)) as RightHandExpression
      )
      .WithContext(new ParseContext())
      .Named("NumberLiteral");

  public static TextParser<(RightHandExpression value, ParseContext context)> StringLiteralParser =
    CString
      .WithSpan()
      .Select(result =>
        (
          new StringLiteralExpression(result.span, result.value.value) as RightHandExpression,
          result.value.context
        )
      )
      .Named("StringLiteral");
}
