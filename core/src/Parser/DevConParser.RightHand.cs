using CriusNyx.Util;
using DevCon.AST;
using Superpower;
using Superpower.Model;
using SParse = Superpower.Parse;

namespace DevCon.Parser;

public static partial class DevConParser
{
  public static TextParser<(
    RightHandExpression value,
    ParseContext context
  )> RightHandExpressionParser = SParse
    .Ref(() => TermOpParser.NotNull())
    .Named("RightHandExpression");

  public static TextParser<(RightHandExpression value, ParseContext context)> ParenParser = (
    from leftParen in DevConToken.LeftParen
    from exp in RightHandExpressionParser.RecoverNullWithContext()
    from rightParen in DevConToken.RightParen.WithEmptyContext().RecoverEmptyWithContext()
    select new ParenExpression(leftParen, exp.value, rightParen.value)
      .AsNotNull<RightHandExpression>()
      .With(ParseContext.Combine(exp.context, rightParen.context))
  ).Named("Paren");

  public static TextParser<(RightHandExpression value, ParseContext context)> UnitParser = SParse
    .OneOf(
      LeftHandExpressionParser.Select(x => (x.value as RightHandExpression, x.context)),
      ParenParser,
      StringLiteralParser,
      NumberLiteralParser
    )
    .Named("Unit");

  public static TextParser<(TextSpan span, UnaryOpType value)> UnaryOpTypeParser = SParse
    .OneOf(
      DevConToken.Exclimation.Select((span) => span.With(UnaryOpType.BooleanNegate)),
      DevConToken.Minus.Select((span) => span.With(UnaryOpType.RealNegate))
    )
    .Named("UnaryOpType");

  public static TextParser<(RightHandExpression value, ParseContext context)> UnaryOpParser = (
    from opType in UnaryOpTypeParser
    from unit in UnitParser.RecoverNullWithContext()
    select new UnaryOp(opType.span, opType.value, unit.value)
      .AsNotNull<RightHandExpression>()
      .With(unit.context)
  ).Named("UnaryOp");

  public static TextParser<(TextSpan span, BinaryOpType value)> FactorOpTypeParser = SParse
    .OneOf(
      DevConToken.Asterisk.Select(span => span.With(BinaryOpType.Multiply)),
      DevConToken.FSlash.Select(span => span.With(BinaryOpType.Divide)),
      DevConToken.Percent.Select(span => span.With(BinaryOpType.Modulo))
    )
    .Named("FactorOpType");

  private static TextParser<(RightHandExpression value, ParseContext context)> FactorOperandParser =
    UnaryOpParser.Or(UnitParser);

  public static TextParser<(RightHandExpression value, ParseContext context)> FactorOpParser =
    FactorOperandParser
      .Named("FactorOperandFirst")
      .ThenChain(
        FactorOpTypeParser.Named("FactorOperator"),
        FactorOperandParser.Named("FactorOperandRest").RecoverNullWithContext(),
        (op, left, right) =>
          new BinaryOp(op.span, op.value, left.value, right.value).With(
            ParseContext.Combine(left.context, right.context)
          )
      )
      .Named("FactorOp");

  public static TextParser<(TextSpan span, BinaryOpType value)> TermOpTypeParser = SParse
    .OneOf(
      DevConToken.Plus.Select(span => span.With(BinaryOpType.Add)),
      DevConToken.Minus.Select(span => span.With(BinaryOpType.Subtract))
    )
    .Named("TermOpType");

  public static TextParser<(RightHandExpression value, ParseContext context)> TermOpParser =
    FactorOpParser
      .Named("FactorOperandFirst")
      .ThenChain(
        TermOpTypeParser.Named("FactorOperator"),
        FactorOpParser.Named("FactorOperandRest").RecoverNullWithContext(),
        (op, left, right) =>
          new BinaryOp(op.span, op.value, left.value, right.value)
            .AsNotNull<RightHandExpression>()
            .With(ParseContext.Combine(left.context, right.context))
      )
      .Named("TermOp");
}
