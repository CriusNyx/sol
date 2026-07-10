using Superpower;
using Superpower.Model;
using DevConIdent = DevCon.AST.Identifier;
using SIdentifier = Superpower.Parsers.Identifier;
using SParser = Superpower.Parsers;
using SSpan = Superpower.Parsers.Span;

namespace DevCon.Parser;

/// <summary>
/// Parsers for language tokens.
/// </summary>
public static class DevConToken
{
  /// <summary>
  /// Non semantic language tokens.
  /// </summary>
  public static TextParser<TextSpan> NonSemantic = SParser
    .Character.Matching((c) => char.IsWhiteSpace(c) && c != '\n', "NonSemantic")
    .Many()
    .WithSpan()
    .Select(x => x.span);

  /// <summary>
  /// identifier
  /// </summary>
  public static TextParser<DevConIdent> Identifier = SIdentifier
    .CStyle.Select(x => new DevConIdent(x))
    .ThenIgnore(NonSemantic)
    .Named("Identifier");

  /// <summary>
  /// =
  /// </summary>
  public static TextParser<TextSpan> Equal = SSpan.EqualTo("=").ThenIgnore(NonSemantic).Named("=");

  /// <summary>
  /// .
  /// </summary>
  public static TextParser<TextSpan> Dot = SSpan.EqualTo(".").ThenIgnore(NonSemantic).Named(".");

  /// <summary>
  /// (
  /// </summary>
  public static TextParser<TextSpan> LeftParen = SSpan
    .EqualTo("(")
    .ThenIgnore(NonSemantic)
    .Named("(");

  /// <summary>
  /// )
  /// </summary>
  public static TextParser<TextSpan> RightParen = SSpan
    .EqualTo(")")
    .ThenIgnore(NonSemantic)
    .Named(")");

  /// <summary>
  /// [
  /// </summary>
  public static TextParser<TextSpan> LeftBracket = SSpan
    .EqualTo("[")
    .ThenIgnore(NonSemantic)
    .Named("[");

  /// <summary>
  /// ]
  /// </summary>
  public static TextParser<TextSpan> RightBracket = SSpan
    .EqualTo("]")
    .ThenIgnore(NonSemantic)
    .Named("]");

  /// <summary>
  /// !
  /// </summary>
  public static TextParser<TextSpan> Exclimation = SSpan
    .EqualTo("!")
    .ThenIgnore(NonSemantic)
    .Named("!");

  /// <summary>
  /// +
  /// </summary>
  public static TextParser<TextSpan> Plus = SSpan.EqualTo("+").ThenIgnore(NonSemantic).Named("+");

  /// <summary>
  /// -
  /// </summary>
  public static TextParser<TextSpan> Minus = SSpan.EqualTo("-").ThenIgnore(NonSemantic).Named("-");

  /// <summary>
  /// *
  /// </summary>
  public static TextParser<TextSpan> Asterisk = SSpan
    .EqualTo("*")
    .ThenIgnore(NonSemantic)
    .Named("*");

  /// <summary>
  /// /
  /// </summary>
  public static TextParser<TextSpan> FSlash = SSpan.EqualTo("/").ThenIgnore(NonSemantic).Named("/");

  /// <summary>
  /// %
  /// </summary>
  public static TextParser<TextSpan> Percent = SSpan
    .EqualTo("%")
    .ThenIgnore(NonSemantic)
    .Named("%");

  /// <summary>
  /// ;
  /// </summary>
  public static TextParser<TextSpan> Semicolon = SSpan
    .EqualTo(";")
    .ThenIgnore(NonSemantic)
    .Named(";");

  /// <summary>
  /// ,
  /// </summary>
  public static TextParser<TextSpan> Comma = SSpan.EqualTo(",").ThenIgnore(NonSemantic).Named(",");

  /// <summary>
  /// use
  /// </summary>
  public static TextParser<TextSpan> Use = SSpan
    .EqualTo("use")
    .ThenIgnore(NonSemantic)
    .Named("use");

  /// <summary>
  /// \n
  /// </summary>
  public static TextParser<TextSpan> NewLine = SSpan
    .EqualTo("\n")
    .ThenIgnore(NonSemantic)
    .Named("new line");

  /// <summary>
  /// EOF
  /// </summary>
  public static TextParser<TextSpan> EOF = delegate(TextSpan input)
  {
    if (input.IsAtEnd)
    {
      return Result.Value(TextSpan.Empty, input, TextSpan.Empty);
    }
    return Result.Empty<TextSpan>(TextSpan.Empty);
  };

  /// <summary>
  /// \n | EOF
  /// </summary>
  public static TextParser<TextSpan> LineTerminator = Parse.OneOf(NewLine, EOF);

  /// <summary>
  /// Recovery parser for tokens.
  /// </summary>
  public static TextParser<TextSpan> RecoveryParser = Parse
    .OneOf(SSpan.Except("\n"))
    .OptionalOrDefault();
}
