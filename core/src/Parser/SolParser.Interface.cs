using Sol.AST;
using Superpower;

namespace Sol.Parser;

public static partial class SolParser
{
  public static ASTNode Parse(string source)
  {
    return Parse(source, ProgramParser);
  }

  public static T Parse<T>(string source, TextParser<T> parser)
  {
    return parser.Parse(source);
  }

  public static bool TryParse(string source, out ASTNode result)
  {
    return TryParse(source, ProgramParser.Select(x => x as ASTNode), out result);
  }

  public static bool TryParse<T>(string source, TextParser<T> parser, out T result)
  {
    var output = parser.TryParse(source);
    result = output.HasValue ? output.Value : default!;
    return output.HasValue;
  }
}
