using DevCon.AST;
using Superpower;
using SParser = Superpower.Parsers;

namespace DevCon.Parser;

public partial class DevConParser
{
  public static TextParser<(RightHandExpression value, ParseContext context)> NumberLiteralParser =
    SParser
      .Numerics.DecimalDecimal.ThenIgnore(DevConToken.NonSemantic)
      .WithSpan()
      .Select(
        (result) =>
          new NumberLiteralExpression(result.span, new NumVal(result.value)) as RightHandExpression
      )
      .WithContext(new ParseContext());

  public static TextParser<(RightHandExpression value, ParseContext context)> StringLiteralParser =
    SParser
      .QuotedString.CStyle.WithSpan()
      .Select(result =>
        new StringLiteralExpression(result.span, result.value) as RightHandExpression
      )
      .WithContext(new ParseContext());
}
