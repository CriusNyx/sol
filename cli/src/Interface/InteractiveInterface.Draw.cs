using Sharpie.Abstractions;

namespace DevCon.CLI;

public partial class InteractiveInterface
{
  const int DebuggerSize = 20;

  private void Draw(IScreen window)
  {
    window.Clear();
    window.Padding(
      1,
      (screen) =>
      {
        var size = screen.Size;

        screen.Screen.SplitV(size.Height - 1, DrawMiddle, DrawBottom);
      }
    );

    window.Refresh();
  }

  private void DrawMiddle(IWindow middle)
  {
    if (state.showDebugger)
    {
      var half = middle.Size.Width / 2;
      middle.Screen.SplitH(half, DrawMain, DrawDebugger);
    }
    else
    {
      DrawMain(middle);
    }
  }

  private void DrawBottom(IWindow screen)
  {
    screen.WriteText("$: ");
    screen.WriteText(state.currentCommandLine);
  }

  private void DrawMain(IWindow screen)
  {
    foreach (var message in MainLog)
    {
      screen.WriteText(message + "\n");
    }
  }

  private void DrawDebugger(IWindow window)
  {
    window.WriteText("Debugger\n");
    foreach (var message in DebugLog)
    {
      window.WriteText(message + "\n");
    }
  }
}
