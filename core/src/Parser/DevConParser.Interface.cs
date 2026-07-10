using CriusNyx.Results;
using CriusNyx.Results.Extensions;
using DevCon.AST;
using Superpower;
using static CriusNyx.Results.Result;

namespace DevCon.Parser;

// This file contains the parts of the parser that are designed to interface with other parts of the program.

/// <summary>
/// Parser for the dev con language,
/// </summary>
public static partial class DevConParser
{
  /// <summary>
  /// Parse the source and return a DevConProgram or CompilerError.
  /// </summary>
  /// <param name="source"></param>
  /// <returns></returns>
  public static Result<DevConProgram, CompilerError> Parse(string source)
  {
    return Parse(source, ProgramParser);
  }

  /// <summary>
  /// Parse a particular grammar element. Return that element or a CompilerError.
  /// Normally this should only be used by unit tests.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="source"></param>
  /// <param name="parser"></param>
  /// <returns></returns>
  public static Result<T, CompilerError> Parse<T>(
    string source,
    TextParser<(T, ParseContext)> parser
  )
    where T : ASTNode
  {
    var (result, context) = ParseWithContext(source, parser);
    if (context.HasError)
    {
      return Err<T, CompilerError>(new CompilerError(result, context.AsOption()));
    }
    return Ok<T, CompilerError>(result);
  }

  /// <summary>
  /// Parse the program and return the program and context.
  /// </summary>
  /// <param name="source"></param>
  /// <returns></returns>
  public static (DevConProgram, ParseContext) ParseWithContext(string source)
  {
    return ParseWithContext(source, ProgramParser);
  }

  /// <summary>
  /// Parse the program and return the program and context.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="source"></param>
  /// <param name="parser"></param>
  /// <returns></returns>
  public static (T, ParseContext) ParseWithContext<T>(
    string source,
    TextParser<(T, ParseContext)> parser
  )
  {
    return parser.Parse(source);
  }
}
