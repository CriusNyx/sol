using CriusNyx.Util;
using Sol.Parser;

public partial class InteractiveInterface
{
  ExecutionContext context = new ExecutionContext();

  public void InterpretState()
  {
    var line = state.currentCommandLine;
    state.currentCommandLine = "";
    try
    {
      var parsed = SolParser.Parse(line);
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
