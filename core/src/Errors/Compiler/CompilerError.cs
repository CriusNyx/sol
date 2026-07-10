using CriusNyx.Results;
using DevCon.AST;
using DevCon.Parser;
using DevCon.TypeSystem;

namespace DevCon;

/// <summary>
/// Error value returned when the program fails to compile.
/// </summary>
public class CompilerError
{
  /// <summary>
  /// The AST or partial AST node from the program.
  /// </summary>
  public readonly ASTNode ast;

  /// <summary>
  /// The context used to parse the program.
  /// Parse errors will be inside the parse context.
  /// Will be None if the compiler failed before parsing.
  /// </summary>
  public readonly Option<ParseContext> parseContext;

  /// <summary>
  /// The context used to type check the program.
  /// Type checking errors will be inside the parse context.
  /// Will be None if the program failed before type checking.
  /// </summary>
  public readonly Option<TypeContext> typeContext;

  public CompilerError(
    ASTNode ast,
    Option<ParseContext>? parseContext = null,
    Option<TypeContext>? typeContext = null
  )
  {
    this.ast = ast;
    this.parseContext = parseContext ?? Option.None<ParseContext>();
    this.typeContext = typeContext ?? Option.None<TypeContext>();
  }

  /// <summary>
  /// Enumerate the parse errors from the parse context, or safely return an empty array.
  /// </summary>
  public IEnumerable<ParseError> ParseErrors => parseContext.Map((x) => x.Errors).UnwrapOr([]);
}
