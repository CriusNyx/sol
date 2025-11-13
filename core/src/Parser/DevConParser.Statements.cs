using CriusNyx.Util;
using DevCon.AST;
using Superpower;
using SParse = Superpower.Parse;
using SSpan = Superpower.Parsers.Span;

namespace DevCon.Parser;

public static partial class DevConParser
{
  public static TextParser<(ASTNode value, ParseContext context)> AssignParser = (
    from left in LeftHandExpressionParser
    from equalSym in DevConToken.Equal.Try()
    from right in RightHandExpressionParser.NotNull().RecoverNullWithContext()
    select new Assign(left.value, equalSym, right.value)
      .AsNotNull<ASTNode>()
      .With(ParseContext.Combine(left.context, right.context))
  ).Named("AssignStatement");

  public static TextParser<(ASTNode value, ParseContext context)> UseParser = (
    from useKeyword in DevConToken.Use
    from nsIdentifiers in DevConToken
      .Identifier.SeparatedBy(DevConToken.Dot)
      .Where(x => x.Length > 0, "Cannot use an empty namespace.")
      .WithEmptyContext()
      .RecoverNullWithContext()
    select new UseStatement(new(useKeyword), nsIdentifiers.value?.ToArray()!)
      .AsNotNull<ASTNode>()
      .With(nsIdentifiers.context)
  ).Named("UseStatement");

  // TODO: This does not look correct.
  public static TextParser<(ASTNode value, ParseContext context)> EmptyParser = SSpan
    .EqualTo("\n")
    .Select(x => new EmptyStatement(x).AsNotNull<ASTNode>().With(new ParseContext()))
    .Named("EmptyStatement");

  public static TextParser<(ASTNode value, ParseContext context)> StatementParser = SParse
    .OneOf(
      UseParser.AsStatementParser(),
      AssignParser.AsStatementParser(),
      RightHandExpressionParser.NotNull().AsStatementParser(),
      EmptyParser.AsStatementParser()
    )
    .Named("Statement");

  public static TextParser<(ASTNode value, ParseContext context)> AsStatementParser<T>(
    this TextParser<(T value, ParseContext context)> source
  )
    where T : ASTNode
  {
    return from exp in source
      from lineTerminator in DevConToken
        .LineTerminator.WithEmptyContext()
        .RecoverUntilWithContext(DevConToken.LineTerminator)
      select exp
        .value.As<ASTNode>()
        .With(ParseContext.Combine(exp.context, lineTerminator.context));
  }
}
