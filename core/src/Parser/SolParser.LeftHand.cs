using System.Reflection.Metadata;
using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using SParse = Superpower.Parse;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<LeftHandExpressionChain> ChainExpressionParser = SParse.Ref(() =>
    SParse.OneOf(DerefParser, InvocationParser, Deindex)
  );

  public static TextParser<LeftHandExpressionChain> DerefParser =>
    SolToken
      .Dot.IgnoreThen(SolToken.Identifier)
      .Then(
        (ident) =>
          ChainExpressionParser!
            .OptionalOrDefault()
            .Select(chain => new DerefExpression(ident, chain) as LeftHandExpressionChain)
      );

  public static TextParser<LeftHandExpressionChain> Deindex =>
    from leftBracket in SolToken.LeftBracket
    from index in RightHandExpressionParser
    from rightBracket in SolToken.RightBracket
    from chain in ChainExpressionParser!.OptionalOrDefault()
    select new DeindexExpression(new(leftBracket), index, new(rightBracket), chain)
      as LeftHandExpressionChain;

  public static TextParser<LeftHandExpressionChain> InvocationParser =>
    from leftParen in SolToken.LeftParen
    from args in RightHandExpressionParser.SeparatedBy(SolToken.Comma).OptionalOrDefault([])
    from rightParen in SolToken.RightParen
    from chain in ChainExpressionParser!.OptionalOrDefault()
    select new InvocationExpression(new(leftParen), args, new(rightParen), chain)
      as LeftHandExpressionChain;

  public static TextParser<LeftHandExpression> LeftHandExpressionParser = SParse.Ref(() =>
    SolToken.Identifier.Then(
      (ident) =>
        ChainExpressionParser!
          .OptionalOrDefault()
          .Select(chain => new LeftHandExpression(ident, chain))
    )
  );
}
