using Sol.AST;
using Superpower;

namespace Sol.Parser;

public static partial class SolParser
{
  public static SolProgram Parse(string source)
  {
    return Parse(source, ProgramParser).value;
  }

  public static T Parse<T>(string source, TextParser<T> parser)
  {
    return parser.Parse(source);
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

  public static bool TryParse(string source, out ASTNode result)
  {
    return TryParse(source, out result, out _);
  }

  public static bool TryParse(string source, out ASTNode result, out ParseContext context)
  {
    var output = TryParse(source, ProgramParser, out var parserResult);

    result = output ? parserResult.value : default!;
    context = output ? parserResult.context : default!;

    return output;
  }

  public static bool TryParse<T>(string source, TextParser<T> parser, out T result)
  {
    var output = parser.TryParse(source);
    result = output.HasValue ? output.Value : default!;
    return output.HasValue;
  }
}
