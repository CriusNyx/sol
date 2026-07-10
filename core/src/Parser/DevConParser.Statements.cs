using CriusNyx.Util;
using DevCon.AST;
using Superpower;
using SParse = Superpower.Parse;

namespace DevCon.Parser;

public static partial class DevConParser
{
  /// <summary>
  /// Assign Parser.
  ///   <grammar>
  ///     AssignStatement -> LeftHandExpression equalSign RightHandExpression
  ///   </grammar>
  /// </summary>
  public static TextParser<(ASTNode value, ParseContext context)> AssignParser = (
    from left in LeftHandExpressionParser
    from equalSym in DevConToken.Equal.Try()
    from right in RightHandExpressionParser.NotNull().RecoverNullWithContext()
    select new Assign(left.value, equalSym, right.value)
      .AsNotNull<ASTNode>()
      .With(ParseContext.Combine(left.context, right.context))
  ).Named("AssignStatement");

  /// <summary>
  /// Use Parser.
  ///   <grammar>
  ///     UseStatement -> useKeyword Identifier [dot Identifier]*
  ///   </grammar>
  /// </summary>
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

  /// <summary>
  /// Parser for an empty statement
  /// </summary>
  public static TextParser<(ASTNode value, ParseContext context)> EmptyParser = DevConToken
    .NonSemantic.WithSpan()
    .Then((_) => DevConToken.NewLine)
    .WithSpan()
    .Select((x) => new EmptyStatement(x.span).AsNotNull<ASTNode>().With(new ParseContext()))
    .Named("EmptyStatement");

  /// <summary>
  /// Parser for statement
  ///   <grammar>
  ///     Statement -> UseStatement | AssignStatement | RightHandExpression | EmptyStatement
  ///   </grammar>
  /// </summary>
  public static TextParser<(ASTNode value, ParseContext context)> StatementParser = SParse
    .OneOf(
      UseParser.AsStatementParser(),
      AssignParser.AsStatementParser(),
      RightHandExpressionParser.NotNull().AsStatementParser(),
      EmptyParser
    )
    .Named("Statement");

  /// <summary>
  /// Convert parser to a stement parser with recovery.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="source"></param>
  /// <returns></returns>
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
