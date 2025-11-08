using Sol.AST;
using Superpower;
using SParser = Superpower.Parsers;

namespace Sol.Parser;

public partial class SolParser
{
  public static TextParser<(RightHandExpression value, ParseContext context)> NumberLiteralParser =
    SParser
      .Numerics.DecimalDecimal.ThenIgnore(SolToken.NonSemantic)
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
