using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using SParser = Superpower.Parsers;

namespace Sol.Parser;

public partial class SolParser
{
  public static TextParser<RightHandExpression> NumberLiteralParser =>
    SParser
      .Numerics.DecimalDecimal.WithSpan()
      .Select(
        (result) =>
          new NumberLiteralExpression(new(result.span), new NumVal(result.value))
          as RightHandExpression
      );

  public static TextParser<RightHandExpression> StringLiteralParser =>
    SParser
      .QuotedString.CStyle.WithSpan()
      .Select(result =>
        new StringLiteralExpression(new(result.span), result.value) as RightHandExpression
      );
}
