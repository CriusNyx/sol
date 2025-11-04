using CriusNyx.Util;
using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using Superpower.Model;
using SParse = Superpower.Parse;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<(
    RightHandExpression value,
    ParseContext context
  )> RightHandExpressionParser = SParse.Ref(() => TermOpParser.NotNull());

  public static TextParser<(RightHandExpression value, ParseContext context)> ParenParser =
    from leftParen in SolToken.LeftParen
    from exp in RightHandExpressionParser
    from rightParen in SolToken.RightParen
    select new ParenExpression(new(leftParen), exp.value, new(rightParen))
      .AsNotNull<RightHandExpression>()
      .With(exp.context);

  public static TextParser<(RightHandExpression value, ParseContext context)> UnitParser =
    SParse.OneOf(
      LeftHandExpressionParser.Select(x => (x.value as RightHandExpression, x.context)),
      ParenParser,
      StringLiteralParser,
      NumberLiteralParser
    );

  public static TextParser<(TextSpan span, UnaryOpType value)> UnaryOpTypeParser = SParse.OneOf(
    SolToken.Exclimation.Select((span) => span.With(UnaryOpType.BooleanNegate)),
    SolToken.Minus.Select((span) => span.With(UnaryOpType.RealNegate))
  );

  public static TextParser<(RightHandExpression value, ParseContext context)> UnaryOpParser =
    from opType in UnaryOpTypeParser
    from unit in UnitParser.RecoverNullWithContext()
    select new UnaryOp(new(opType.span), opType.value, unit.value)
      .AsNotNull<RightHandExpression>()
      .With(unit.context);

  public static TextParser<(TextSpan span, BinaryOpType value)> FactorOpTypeParser = SParse.OneOf(
    SolToken.Asterisk.Select(span => span.With(BinaryOpType.Multiply)),
    SolToken.FSlash.Select(span => span.With(BinaryOpType.Divide)),
    SolToken.Percent.Select(span => span.With(BinaryOpType.Modulo))
  );

  private static TextParser<(RightHandExpression value, ParseContext context)> FactorOperandParser =
    UnaryOpParser.Or(UnitParser);

  public static TextParser<(RightHandExpression value, ParseContext context)> FactorOpParser =
    FactorOperandParser
      .Named("FactorOperandFirst")
      .ThenChain(
        FactorOpTypeParser.Named("FactorOperator"),
        FactorOperandParser.Named("FactorOperandRest").RecoverNullWithContext(),
        (op, left, right) =>
          new BinaryOp(new(op.span), op.value, left.value, right.value).With(
            ParseContext.Combine(left.context, right.context)
          )
      );

  public static TextParser<(TextSpan span, BinaryOpType value)> TermOpTypeParser = SParse.OneOf(
    SolToken.Plus.Select(span => span.With(BinaryOpType.Add)),
    SolToken.Minus.Select(span => span.With(BinaryOpType.Subtract))
  );

  public static TextParser<(RightHandExpression value, ParseContext context)> TermOpParser =
    FactorOpParser
      .Named("FactorOperandFirst")
      .ThenChain(
        TermOpTypeParser.Named("FactorOperator"),
        FactorOpParser.Named("FactorOperandRest").RecoverNullWithContext(),
        (op, left, right) =>
          new BinaryOp(new(op.span), op.value, left.value, right.value)
            .AsNotNull<RightHandExpression>()
            .With(ParseContext.Combine(left.context, right.context))
      );
}
