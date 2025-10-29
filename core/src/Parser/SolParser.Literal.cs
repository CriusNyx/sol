using System.Security.Cryptography;
using Sol.AST;
using Superpower;
using SParser = Superpower.Parsers;

namespace Sol.Parser;

public partial class SolParser
{
  public static TextParser<RightHandExpression> NumberLiteralParser =>
    SParser.Numerics.DecimalDecimal.Select(x =>
      new NumberLiteralExpression(new NumVal(x)) as RightHandExpression
    );

  public static TextParser<RightHandExpression> StringLiteralParser =>
    SParser.QuotedString.CStyle.Select(x => new StringLiteralExpression(x) as RightHandExpression);
}
