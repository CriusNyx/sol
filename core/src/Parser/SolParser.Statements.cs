using CriusNyx.Util;
using Sol.AST;
using Superpower;
using SParse = Superpower.Parse;
using SSpan = Superpower.Parsers.Span;

namespace Sol.Parser;

public static partial class SolParser
{
  public static TextParser<(ASTNode value, ParseContext context)> AssignParser =
    from left in LeftHandExpressionParser
    from equalSym in SolToken.Equal.Try()
    from right in RightHandExpressionParser.RecoverNullWithContext()
    select new Assign(left.value, equalSym, right.value)
      .AsNotNull<ASTNode>()
      .With(ParseContext.Combine(left.context, right.context));

  public static TextParser<(ASTNode value, ParseContext context)> UseParser =
    from useKeyword in SolToken.Use
    from nsIdentifiers in SolToken
      .Identifier.SeparatedBy(SolToken.Dot)
      .Where(x => x.Length > 0, "Cannot use an empty namespace.")
      .WithEmptyContext()
      .RecoverNullWithContext()
    select new UseStatement(new(useKeyword), nsIdentifiers.value?.ToArray()!)
      .AsNotNull<ASTNode>()
      .With(nsIdentifiers.context);

  // TODO: This does not look correct.
  public static TextParser<(ASTNode value, ParseContext context)> EmptyParser = SSpan
    .EqualTo("\n")
    .Select(x => new EmptyStatement(x).AsNotNull<ASTNode>().With(new ParseContext()));

  public static TextParser<(ASTNode value, ParseContext context)> StatementParser = SParse.OneOf(
    UseParser.AsStatementParser(),
    AssignParser.AsStatementParser(),
    RightHandExpressionParser.AsStatementParser(),
    EmptyParser.AsStatementParser()
  );

  public static TextParser<(ASTNode value, ParseContext context)> AsStatementParser<T>(
    this TextParser<(T value, ParseContext context)> source
  )
    where T : ASTNode
  {
    return from exp in source
      from lineTerminator in SolToken
        .LineTerminator.WithEmptyContext()
        .RecoverUntilWithContext(SolToken.LineTerminator)
      select exp
        .value.As<ASTNode>()
        .With(ParseContext.Combine(exp.context, lineTerminator.context));
  }
}
