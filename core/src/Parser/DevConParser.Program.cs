using CriusNyx.Util;
using DevCon.AST;
using Superpower;

namespace DevCon.Parser;

public partial class DevConParser
{
  public static TextParser<(DevConProgram value, ParseContext context)> ProgramParser =>
    (
      StatementParser
        .Many()
        .AtEnd()
        .Select(statements =>
          new DevConProgram(statements.Select(x => x.value).ToArray()).With(
            ParseContext.Combine(statements.Select(x => x.context))
          )
        )
        .RecoverNullWithContext()
    ).Named("Program");
}
