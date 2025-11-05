using Sol.AST;
using Sol.DataStructures;
using Superpower;
using static Sol.DataStructures.Result;

namespace Sol.Parser;

public static partial class SolParser
{
  public static Result<SolProgram, CompilerError> Parse(string source)
  {
    return Parse(source, ProgramParser);
  }

  public static Result<T, CompilerError> Parse<T>(
    string source,
    TextParser<(T, ParseContext)> parser
  )
    where T : ASTNode
  {
    var (result, context) = ParseWithContext(source, parser);
    if (context.HasError)
    {
      return Err<T, CompilerError>(CompilerError.From(result, context));
    }
    return Ok<T, CompilerError>(result);
  }

  public static (SolProgram, ParseContext) ParseWithContext(string source)
  {
    return ParseWithContext(source, ProgramParser);
  }

  public static (T, ParseContext) ParseWithContext<T>(
    string source,
    TextParser<(T, ParseContext)> parser
  )
  {
    return parser.Parse(source);
  }
}
