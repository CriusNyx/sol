using CriusNyx.Util;
using DevCon.AST;
using Superpower;

namespace DevCon.Parser;

public partial class DevConParser
{
  /// <summary>
  /// Parser for a dev con program.
  /// <grammar>
  /// DevConProgram -> Statement*
  /// </grammar>
  /// </summary>
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
        .RecoverValueWithContext(new DevConProgram([]))
    ).Named("Program");
}
