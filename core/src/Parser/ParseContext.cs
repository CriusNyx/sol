using CriusNyx.Util;
using Sol.AST;
using Superpower.Model;

namespace Sol.Parser;

public class ParseError(Result<ASTNode> result)
{
  public Result<ASTNode> SuperpowerResult => result;

  public static ParseError From<T>(Result<T> result)
  {
    return new ParseError(Result.CastEmpty<T, ASTNode>(result));
  }
}

public class ParseContext
{
  private ParseContext[] Children = [];
  private List<ParseError> errors = new List<ParseError>();

  public ParseContext(ParseContext[] children, params ParseError[] errors)
  {
    this.Children = children;
    this.errors = errors.ToList();
  }

  public ParseContext(params ParseError[] errors)
    : this([], errors) { }

  public static ParseContext Combine(IEnumerable<ParseContext> args)
  {
    return new ParseContext(args.WhereAs<ParseContext>().ToArray());
  }

  public static ParseContext Combine(params ParseContext[] args)
  {
    return new ParseContext(args.WhereAs<ParseContext>().ToArray());
  }

  public IEnumerable<ParseError> Errors => errors.Concat(Children.SelectMany(x => x.Errors));
}
