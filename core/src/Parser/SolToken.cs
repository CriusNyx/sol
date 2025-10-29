using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using SIdentifier = Superpower.Parsers.Identifier;
using SolIdent = Sol.AST.Identifier;

namespace Sol.Parser;

public static class SolToken
{
  public static TextParser<TextSpan[]> NonSemantic = Parse.OneOf(Span.WhiteSpace).Many();
  public static TextParser<SolIdent> Identifier = SIdentifier
    .CStyle.Select(x => new SolIdent(x))
    .ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Equal = Span.EqualTo("=").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Dot = Span.EqualTo(".").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> LeftParen = Span.EqualTo("(").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> RightParen = Span.EqualTo(")").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> LeftBracket = Span.EqualTo("[").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> RightBracket = Span.EqualTo("]").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Exclimation = Span.EqualTo("!").ThenIgnore(NonSemantic);

  public static TextParser<TextSpan> Plus = Span.EqualTo("+").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Minus = Span.EqualTo("-").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Asterisk = Span.EqualTo("*").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> FSlash = Span.EqualTo("/").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Percent = Span.EqualTo("%").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Semicolon = Span.EqualTo(";").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Comma = Span.EqualTo(",").ThenIgnore(NonSemantic);
  public static TextParser<TextSpan> Use = Span.EqualTo("use").ThenIgnore(NonSemantic);
}
