namespace Sol.CLI;

public partial class InteractiveInterface
{
  const int MaxLogSize = 100;
  public Queue<string> MainLog = new Queue<string>();
  public Queue<string> DebugLog = new Queue<string>();

  public void LogMain(string message)
  {
    MainLog.Enqueue(message);
    while (MainLog.Count > MaxLogSize)
    {
      MainLog.Dequeue();
    }
  }

  public void LogMessage(string message)
  {
    DebugLog.Enqueue(message);
    while (DebugLog.Count > MaxLogSize)
    {
      DebugLog.Dequeue();
    }
  }
}
