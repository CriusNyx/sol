using CriusNyx.Util;
using Sol.AST;
using Superpower;
using SParse = Superpower.Parse;

namespace Sol.Parser;

public static partial class SolParser
{
  /// <summary>
  /// Chain -> Deref | Invocation | Deindex
  /// </summary>
  public static TextParser<(
    LeftHandExpressionChain value,
    ParseContext context
  )> ChainExpressionParser = SParse.Ref(() =>
    SParse.OneOf(DerefParser.NotNull(), InvocationParser.NotNull(), Deindex.NotNull())
  );

  /// <summary>
  /// Deref -> dot identifier Chain?
  /// </summary>
  public static TextParser<(LeftHandExpressionChain value, ParseContext parseContext)> DerefParser =
    from dot in SolToken.Dot
    from ident in SolToken.Identifier.WithEmptyContext().RecoverNullWithContext()
    from chain in ChainExpressionParser.OptionalOrDefault().RecoverNullWithContext()
    select new DerefExpression(dot, ident.value, chain.value)
      .AsNotNull<LeftHandExpressionChain>()
      .With(ParseContext.Combine(ident.context, chain.context));

  /// <summary>
  /// Deindex -> leftBracket RightHandExpression rightBracket Chain?
  /// </summary>
  public static TextParser<(LeftHandExpressionChain value, ParseContext context)> Deindex =
    from leftBracket in SolToken.LeftBracket
    from index in RightHandExpressionParser.NotNull().RecoverNullWithContext()
    from rightBracket in SolToken.RightBracket.WithEmptyContext().RecoverEmptyWithContext()
    from chain in ChainExpressionParser.OptionalOrDefault()
    select new DeindexExpression(leftBracket, index.value, rightBracket.value, chain.value)
      .AsNotNull<LeftHandExpressionChain>()
      .With(ParseContext.Combine(index.context, rightBracket.context, chain.context));

  public static TextParser<(
    RightHandExpression[] value,
    ParseContext context
  )> InvocationArgParser =>
    RightHandExpressionParser
      .SeparatedBy(
        SolToken.Comma,
        parser => parser.RecoverUntilWithContext(SolToken.Comma, SolToken.RightParen)
      )
      .OptionalOrDefault([])
      .Select(result =>
        result
          .Select(item => item.value)
          .ToArray()
          .With(ParseContext.Combine(result.Select(item => item.context)))
      );

  /// <summary>
  /// Invocation -> leftParen ((Expression comma)* Expression)? rightParen Chain?
  /// </summary>
  public static TextParser<(LeftHandExpressionChain value, ParseContext context)> InvocationParser =
    from leftParen in SolToken.LeftParen
    from args in InvocationArgParser.RecoverUntilWithContext(SolToken.RightParen)
    from rightParen in SolToken.RightParen.WithEmptyContext().RecoverEmptyWithContext()
    from chain in ChainExpressionParser!.OptionalOrDefault()
    select new InvocationExpression(leftParen, args.value, rightParen.value, chain.value)
      .AsNotNull<LeftHandExpressionChain>()
      .With(ParseContext.Combine(args.context, rightParen.context, chain.context));

  /// <summary>
  /// LeftHandExpression -> ident Chain?
  /// </summary>
  public static TextParser<(
    LeftHandExpression value,
    ParseContext context
  )> LeftHandExpressionParser = SParse.Ref(() =>
    from ident in SolToken.Identifier
    from chain in ChainExpressionParser!.OptionalOrDefault()
    select new LeftHandExpression(ident, chain.value)
      .AsNotNull<LeftHandExpression>()
      .With(chain.context)
  );
}
