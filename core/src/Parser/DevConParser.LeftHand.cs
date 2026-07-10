using CriusNyx.Util;
using DevCon.AST;
using Superpower;
using SParse = Superpower.Parse;

namespace DevCon.Parser;

// Contains grammar elements for LeftHandExpressions.

public static partial class DevConParser
{
  /// <summary>
  /// <grammar>
  /// Chain -> Deref | Invocation | Deindex
  /// </grammar>
  /// </summary>
  public static TextParser<(
    LeftHandExpressionChain value,
    ParseContext context
  )> ChainExpressionParser = SParse
    .Ref(() =>
      SParse.OneOf(DerefParser.NotNull(), InvocationParser.NotNull(), DeindexParser.NotNull())
    )
    .Named("ChainExpression");

  /// <summary>
  /// <grammar>
  /// Deref -> dot identifier Chain?
  /// </grammar>
  /// </summary>
  public static TextParser<(LeftHandExpressionChain value, ParseContext parseContext)> DerefParser =
    (
      from dot in DevConToken.Dot
      from ident in DevConToken.Identifier.WithEmptyContext().RecoverNullWithContext()
      from chain in ChainExpressionParser.OptionalOrDefault().RecoverNullWithContext()
      select new DerefExpression(dot, ident.value, chain.value)
        .AsNotNull<LeftHandExpressionChain>()
        .With(ParseContext.Combine(ident.context, chain.context))
    ).Named("Deref");

  /// <summary>
  /// <grammar>
  /// Deindex -> leftBracket RightHandExpression rightBracket Chain?
  /// </grammar>
  /// </summary>
  public static TextParser<(LeftHandExpressionChain value, ParseContext context)> DeindexParser = (
    from leftBracket in DevConToken.LeftBracket
    from index in RightHandExpressionParser.NotNull().RecoverNullWithContext()
    from rightBracket in DevConToken.RightBracket.WithEmptyContext().RecoverEmptyWithContext()
    from chain in ChainExpressionParser.OptionalOrDefault()
    select new DeindexExpression(leftBracket, index.value, rightBracket.value, chain.value)
      .AsNotNull<LeftHandExpressionChain>()
      .With(ParseContext.Combine(index.context, rightBracket.context, chain.context))
  ).Named("Deindex");

  /// <summary>
  ///
  /// </summary>
  public static TextParser<(
    RightHandExpression[] value,
    ParseContext context
  )> InvocationArgParser =>
    RightHandExpressionParser
      .SeparatedBy(
        DevConToken.Comma,
        parser => parser.RecoverUntilWithContext(DevConToken.Comma, DevConToken.RightParen)
      )
      .OptionalOrDefault([])
      .Select(result =>
        result
          .Select(item => item.value)
          .ToArray()
          .With(ParseContext.Combine(result.Select(item => item.context)))
      )
      .Named("InvocationArg");

  /// <summary>
  /// <grammar>
  /// Invocation -> leftParen ((Expression comma)* Expression)? rightParen Chain?
  /// </grammar>
  /// </summary>
  public static TextParser<(LeftHandExpressionChain value, ParseContext context)> InvocationParser =
    (
      from leftParen in DevConToken.LeftParen
      from args in InvocationArgParser.RecoverUntilWithContext(DevConToken.RightParen)
      from rightParen in DevConToken.RightParen.WithEmptyContext().RecoverEmptyWithContext()
      from chain in ChainExpressionParser!.OptionalOrDefault()
      select new InvocationExpression(leftParen, args.value, rightParen.value, chain.value)
        .AsNotNull<LeftHandExpressionChain>()
        .With(ParseContext.Combine(args.context, rightParen.context, chain.context))
    ).Named("Invocation");

  /// <summary>
  /// <grammar>
  /// LeftHandExpression -> ident Chain?
  /// </grammar>
  /// </summary>
  public static TextParser<(
    LeftHandExpression value,
    ParseContext context
  )> LeftHandExpressionParser = SParse.Ref(() =>
    (
      from ident in DevConToken.Identifier
      from chain in ChainExpressionParser!.OptionalOrDefault()
      select new LeftHandExpression(ident, chain.value)
        .AsNotNull<LeftHandExpression>()
        .With(chain.context)
    ).Named("LeftHandExpression")
  );
}
