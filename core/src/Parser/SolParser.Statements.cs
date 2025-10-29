using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using SParse = Superpower.Parse;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<Assign> AssignParser = LeftHandExpressionParser
    .ThenIgnore(SolToken.Equal)
    .Then((left) => RightHandExpressionParser.Select((right) => new Assign(left, right)));

  public static TextParser<UseExpression> UseParser = SolToken
    .Use.IgnoreThen(SolToken.Identifier.SeparatedBy(SolToken.Dot))
    .Where(x => x.Length > 0, "Cannot use an empty namespace.")
    .Select(x => new UseExpression(x.ToArray()));

  public static TextParser<ASTNode> StatementParser = SParse.OneOf(
    UseParser.Select(x => x as ASTNode).Try(),
    AssignParser.Select(x => x as ASTNode).Try(),
    RightHandExpressionParser.Select(x => x as ASTNode).Try()
  );
}
