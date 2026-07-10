using CriusNyx.Util;
using DevCon.AST;
using Superpower;
using SParse = Superpower.Parse;
using SParser = Superpower.Parsers;

namespace DevCon.Parser;

public static partial class DevConParser
{
  /// <summary>
  /// Parser for CString character.
  /// </summary>
  private static readonly TextParser<char> CStringContentChar = SParser
    .Span.EqualTo("\\\"")
    .Value('"')
    .Try()
    .Or(SParser.Character.ExceptIn('"', '\\', '\r', '\n'));

  /// <summary>
  /// Parser for a String.
  /// Modified version of Superpower parser that supports recovery.
  /// </summary>
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

  /// <summary>
  /// Parser for a number literal.
  /// <grammar>
  /// NumberLiteral
  /// </grammar>
  /// </summary>
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

  /// <summary>
  /// Parser for string literal.
  /// <grammar>
  /// StringLiteral
  /// </grammar>
  /// </summary>
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
