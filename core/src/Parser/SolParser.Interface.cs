using Sol.AST;
using Sol.Parser.Extensions;
using Superpower;

namespace Sol.Parser;

public static partial class SolParser
{
  public static ASTNode Parse(string source)
  {
    return Parse(source, StatementParser);
  }

  public static T Parse<T>(string source, TextParser<T> parser)
  {
    return parser.FullText().Parse(source);
  }

  public static bool TryParse(string source, out ASTNode result)
  {
    return TryParse(source, StatementParser, out result);
  }

  public static bool TryParse<T>(string source, TextParser<T> parser, out T result)
  {
    var output = parser.FullText().TryParse(source);
    result = output.HasValue ? output.Value : default!;
    return output.HasValue;
  }
}
