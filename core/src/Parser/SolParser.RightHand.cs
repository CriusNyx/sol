using CriusNyx.Util;
using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using Superpower.Model;
using SParse = Superpower.Parse;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<RightHandExpression> RightHandExpressionParser = SParse.Ref(() =>
    TermOpParser.NotNull()
  );

  public static TextParser<RightHandExpression> ParenParser =
    from leftParen in SolToken.LeftParen
    from exp in RightHandExpressionParser
    from rightParen in SolToken.RightParen
    select new ParenExpression(new(leftParen), exp, new(rightParen)) as RightHandExpression;

  public static TextParser<RightHandExpression> UnitParser = SParse.OneOf(
    LeftHandExpressionParser.Select(x => x as RightHandExpression),
    ParenParser,
    StringLiteralParser,
    NumberLiteralParser
  );

  public static TextParser<(TextSpan span, UnaryOpType value)> UnaryOpTypeParser = SParse.OneOf(
    SolToken.Exclimation.Select((span) => span.With(UnaryOpType.BooleanNegate)),
    SolToken.Minus.Select((span) => span.With(UnaryOpType.RealNegate))
  );

  public static TextParser<RightHandExpression> UnaryOpParser = UnaryOpTypeParser.Then(
    (op) =>
      UnitParser.Select((unit) => new UnaryOp(new(op.span), op.value, unit) as RightHandExpression)
  );

  public static TextParser<(TextSpan span, BinaryOpType value)> FactorOpTypeParser = SParse.OneOf(
    SolToken.Asterisk.Select(span => span.With(BinaryOpType.Multiply)),
    SolToken.FSlash.Select(span => span.With(BinaryOpType.Divide)),
    SolToken.Percent.Select(span => span.With(BinaryOpType.Modulo))
  );

  public static TextParser<RightHandExpression> FactorOpParser = SParse.Chain(
    FactorOpTypeParser,
    UnaryOpParser.Or(UnitParser),
    (op, left, right) => new BinaryOp(new(op.span), op.value, left, right)
  );

  public static TextParser<(TextSpan span, BinaryOpType value)> TermOpTypeParser = SParse.OneOf(
    SolToken.Plus.Select(span => span.With(BinaryOpType.Add)),
    SolToken.Minus.Select(span => span.With(BinaryOpType.Subtract))
  );

  public static TextParser<RightHandExpression> TermOpParser = SParse.Chain(
    TermOpTypeParser,
    FactorOpParser,
    (op, left, right) => new BinaryOp(new(op.span), op.value, left, right)
  );
}
