using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using Superpower.Model;
using SParse = Superpower.Parse;
using SSpan = Superpower.Parsers.Span;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<Assign> AssignParser =
    from left in LeftHandExpressionParser
    from equalSym in SolToken.Equal.Try()
    from right in RightHandExpressionParser
    select new Assign(left, new(equalSym), right);

  public static TextParser<UseStatement> UseParser =
    from useKeyword in SolToken.Use
    from nsIdentifiers in SolToken
      .Identifier.SeparatedBy(SolToken.Dot)
      .Where(x => x.Length > 0, "Cannot use an empty namespace.")
    select new UseStatement(new(useKeyword), nsIdentifiers.ToArray());

  public static TextParser<EmptyStatement> EmptyParser = SSpan
    .EqualTo("\n")
    .Select(x => new EmptyStatement(x));

  public static TextParser<ASTNode> StatementParser = SParse.OneOf(
    UseParser.Select(x => x as ASTNode).ThenIgnore(SolToken.LineTerminator),
    AssignParser.Select(x => x as ASTNode).ThenIgnore(SolToken.LineTerminator),
    RightHandExpressionParser.Select(x => x as ASTNode).ThenIgnore(SolToken.LineTerminator),
    EmptyParser.Select(x => x as ASTNode).ThenIgnore(SolToken.LineTerminator)
  );
}
