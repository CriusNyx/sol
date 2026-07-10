using CriusNyx.Util;
using DevCon.AST;
using Superpower.Model;

namespace DevCon.Parser;

/// <summary>
/// Parse Error
/// </summary>
/// <param name="result"></param>
public class ParseError(Result<ASTNode> result)
{
  /// <summary>
  /// The AST Node recovered from the error.
  /// </summary>
  public Result<ASTNode> SuperpowerResult => result;

  /// <summary>
  /// Generate an error from a result.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="result"></param>
  /// <returns></returns>
  public static ParseError From<T>(Result<T> result)
  {
    return new ParseError(Result.CastEmpty<T, ASTNode>(result));
  }
}

/// <summary>
/// Context for program parsing. If the parsing was not successful the parse context will include program errors.
/// </summary>
public class ParseContext
{
  /// <summary>
  /// The child contexts of the parse context.
  /// </summary>
  private ParseContext[] Children = [];

  /// <summary>
  /// List of errors assossiated with the context.
  /// </summary>
  private List<ParseError> errors = new List<ParseError>();

  /// <summary>
  /// Create a new parse context.
  /// </summary>
  /// <param name="children"></param>
  /// <param name="errors"></param>
  public ParseContext(ParseContext[] children, params ParseError[] errors)
  {
    this.Children = children;
    this.errors = errors.ToList();
  }

  /// <summary>
  /// Create a new parse context with errors.
  /// </summary>
  /// <param name="errors"></param>
  public ParseContext(params ParseError[] errors)
    : this([], errors) { }

  /// <summary>
  /// Combine multiple parse contextxs together.
  /// </summary>
  /// <param name="args"></param>
  /// <returns></returns>
  public static ParseContext Combine(IEnumerable<ParseContext> args)
  {
    return new ParseContext(args.WhereAs<ParseContext>().ToArray());
  }

  /// <summary>
  /// Combine multiple parse contexts together.
  /// </summary>
  /// <param name="args"></param>
  /// <returns></returns>
  public static ParseContext Combine(params ParseContext[] args)
  {
    return new ParseContext(args.WhereAs<ParseContext>().ToArray());
  }

  /// <summary>
  /// Returns true if the context has any errors.
  /// </summary>
  public bool HasError => Errors.Count() != 0;

  /// <summary>
  /// List of errors the context contains.
  /// </summary>
  public IEnumerable<ParseError> Errors => errors.Concat(Children.SelectMany(x => x.Errors));
}
