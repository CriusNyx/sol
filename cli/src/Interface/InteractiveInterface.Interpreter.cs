using CriusNyx.Util;
using Sol.DataStructures;
using Sol.Parser;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol.CLI;

public partial class InteractiveInterface
{
  ExecutionContext context = new ExecutionContext();

  public void InterpretState()
  {
    var line = state.currentCommandLine;
    state.currentCommandLine = "";
    try
    {
      var parsed = SolParser.Parse(line).Unwrap();
      LogMain(line);
      if (parsed.Evaluate(context) is object result)
      {
        LogMain(result.Debug());
      }
    }
    catch (Exception e)
    {
      LogMain(e.ToString());
    }
  }
}
