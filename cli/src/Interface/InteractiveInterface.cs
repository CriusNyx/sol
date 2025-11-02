using CriusNyx.Util;
using Sharpie;
using Sharpie.Backend;

#pragma warning disable CA1416

class InteractiveInterfaceState
{
  public bool showDebugger;
  public string currentCommandLine = "";
}

public partial class InteractiveInterface : IDisposable
{
  Terminal terminal;
  InteractiveInterfaceState state;

  public InteractiveInterface()
  {
    terminal = new Terminal(CursesBackend.Load().NotNull(), new(ManagedWindows: true));
    state = new InteractiveInterfaceState();
  }

  public void Run()
  {
    foreach (var e in terminal.Events.Listen())
    {
      switch (HandleEvent(e))
      {
        case EventResult.Exit:
          return;
        default:
          Draw(terminal.Screen);
          break;
      }
    }
  }

  public void Dispose()
  {
    terminal.Dispose();
  }
}

#pragma warning restore CA1416
