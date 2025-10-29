using CriusNyx.Util;
using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using SParse = Superpower.Parse;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<RightHandExpression> RightHandExpressionParser = SParse.Ref(() =>
    TermOpParser.NotNull()
  );

  public static TextParser<RightHandExpression> ParenParser = RightHandExpressionParser
    .SurroundedBy(SolToken.LeftParen, SolToken.RightParen)
    .Select(rhe => new ParenExpression(rhe) as RightHandExpression);

  public static TextParser<RightHandExpression> UnitParser = SParse.OneOf(
    LeftHandExpressionParser.Select(x => x as RightHandExpression),
    ParenParser,
    StringLiteralParser,
    NumberLiteralParser
  );

  public static TextParser<UnaryOpType> UnaryOpTypeParser = SParse.OneOf(
    SolToken.Exclimation.Select(_ => UnaryOpType.BooleanNegate),
    SolToken.Minus.Select(_ => UnaryOpType.RealNegate)
  );

  public static TextParser<RightHandExpression> UnaryOpParser = UnaryOpTypeParser.Then(
    (op) => UnitParser.Select((unit) => new UnaryOp(op, unit) as RightHandExpression)
  );

  public static TextParser<BinaryOpType> FactorOpTypeParser = SParse.OneOf(
    SolToken.Asterisk.Select(_ => BinaryOpType.Multiply),
    SolToken.FSlash.Select(_ => BinaryOpType.Divide),
    SolToken.Percent.Select(_ => BinaryOpType.Modulo)
  );

  public static TextParser<RightHandExpression> FactorOpParser = SParse.Chain(
    FactorOpTypeParser,
    UnaryOpParser.Or(UnitParser),
    (op, left, right) => new BinaryOp(op, left, right)
  );

  public static TextParser<BinaryOpType> TermOpTypeParser = SParse.OneOf(
    SolToken.Plus.Select(_ => BinaryOpType.Add),
    SolToken.Minus.Select(_ => BinaryOpType.Subtract)
  );

  public static TextParser<RightHandExpression> TermOpParser = SParse.Chain(
    TermOpTypeParser,
    FactorOpParser,
    (op, left, right) => new BinaryOp(op, left, right)
  );
}
