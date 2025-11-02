using Sol.AST;
using Superpower;

namespace Sol.Parser;

public partial class SolParser
{
  public static TextParser<SolProgram> ProgramParser =>
    StatementParser.Many().AtEnd().Select(statements => new SolProgram(statements));
}
