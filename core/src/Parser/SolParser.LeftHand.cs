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
    SolToken
      .LeftBracket.IgnoreThen(RightHandExpressionParser)
      .ThenIgnore(SolToken.RightBracket)
      .Then(index =>
        ChainExpressionParser!
          .OptionalOrDefault()
          .Select(chain => new DeindexExpression(index, chain) as LeftHandExpressionChain)
      );

  public static TextParser<LeftHandExpressionChain> InvocationParser =>
    RightHandExpressionParser
      .SeparatedBy(SolToken.Comma)
      .OptionalOrDefault([])
      .SurroundedBy(SolToken.LeftParen, SolToken.RightParen)
      .Then(
        (args) =>
          ChainExpressionParser!
            .OptionalOrDefault()
            .Select(chain => new InvocationExpression(args, chain) as LeftHandExpressionChain)
      );

  public static TextParser<LeftHandExpression> LeftHandExpressionParser = SParse.Ref(() =>
    SolToken.Identifier.Then(
      (ident) =>
        ChainExpressionParser!
          .OptionalOrDefault()
          .Select(chain => new LeftHandExpression(ident, chain))
    )
  );
}
