using Sol.Parser.Extensions;
using Superpower;
using Superpower.Model;
using SIdentifier = Superpower.Parsers.Identifier;
using SolIdent = Sol.AST.Identifier;
using SSpan = Superpower.Parsers.Span;

namespace Sol.Parser;

public static class SolToken
{
  public static TextParser<TextSpan[]> NonSemantic = Parse
    .OneOf(SSpan.WithAll((c) => char.IsWhiteSpace(c) && c != '\n'))
    .Many();
  public static TextParser<SolIdent> Identifier = SIdentifier
    .CStyle.Select(x => new SolIdent(new(x)))
    .ThenIgnore(NonSemantic)
    .Named("Identifier");
  public static TextParser<TextSpan> Equal = SSpan.EqualTo("=").ThenIgnore(NonSemantic).Named("=");
  public static TextParser<TextSpan> Dot = SSpan.EqualTo(".").ThenIgnore(NonSemantic).Named(".");
  public static TextParser<TextSpan> LeftParen = SSpan
    .EqualTo("(")
    .ThenIgnore(NonSemantic)
    .Named("(");
  public static TextParser<TextSpan> RightParen = SSpan
    .EqualTo(")")
    .ThenIgnore(NonSemantic)
    .Named(")");
  public static TextParser<TextSpan> LeftBracket = SSpan
    .EqualTo("[")
    .ThenIgnore(NonSemantic)
    .Named("[");
  public static TextParser<TextSpan> RightBracket = SSpan
    .EqualTo("]")
    .ThenIgnore(NonSemantic)
    .Named("]");
  public static TextParser<TextSpan> Exclimation = SSpan
    .EqualTo("!")
    .ThenIgnore(NonSemantic)
    .Named("!");

  public static TextParser<TextSpan> Plus = SSpan.EqualTo("+").ThenIgnore(NonSemantic).Named("+");
  public static TextParser<TextSpan> Minus = SSpan.EqualTo("-").ThenIgnore(NonSemantic).Named("-");
  public static TextParser<TextSpan> Asterisk = SSpan
    .EqualTo("*")
    .ThenIgnore(NonSemantic)
    .Named("*");

  public static TextParser<TextSpan> FSlash = SSpan.EqualTo("/").ThenIgnore(NonSemantic).Named("/");
  public static TextParser<TextSpan> Percent = SSpan
    .EqualTo("%")
    .ThenIgnore(NonSemantic)
    .Named("%");
  public static TextParser<TextSpan> Semicolon = SSpan
    .EqualTo(";")
    .ThenIgnore(NonSemantic)
    .Named(";");
  public static TextParser<TextSpan> Comma = SSpan.EqualTo(",").ThenIgnore(NonSemantic).Named(",");
  public static TextParser<TextSpan> Use = SSpan
    .EqualTo("use")
    .ThenIgnore(NonSemantic)
    .Named("use");
  public static TextParser<TextSpan> NewLine = SSpan
    .EqualTo("\n")
    .ThenIgnore(NonSemantic)
    .Named("new line");
  public static TextParser<TextSpan> EOF = delegate(TextSpan input)
  {
    if (input.IsAtEnd)
    {
      return Result.Value(TextSpan.Empty, input, TextSpan.Empty);
    }
    return Result.Empty<TextSpan>(TextSpan.Empty);
  };

  public static TextParser<TextSpan> LineTerminator = Parse.OneOf(NewLine, EOF);

  public static TextParser<TextSpan> RecoveryParser = Parse
    .OneOf(SSpan.Except("\n"))
    .OptionalOrDefault();
}
