using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.Parser;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.CLI;

public partial class InteractiveInterface
{
  ExecutionContext context = new ExecutionContext();

  public void InterpretState()
  {
    var line = state.currentCommandLine;
    state.currentCommandLine = "";
    try
    {
      var parsed = DevConParser.Parse(line).Unwrap();
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
