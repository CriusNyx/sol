using CriusNyx.Util;
using Sol.AST;
using Superpower;

namespace Sol.Parser;

public partial class SolParser
{
  public static TextParser<(SolProgram value, ParseContext context)> ProgramParser =>
    StatementParser
      .Many()
      .AtEnd()
      .Select(statements =>
        new SolProgram(statements.Select(x => x.value).ToArray()).With(
          ParseContext.Combine(statements.Select(x => x.context))
        )
      );
}
