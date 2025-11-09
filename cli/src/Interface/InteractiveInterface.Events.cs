using Sharpie;

namespace DevCon.CLI;

public enum EventResult
{
  Exit,
  Redraw,
  Backspace,
  Interpret,
}

public partial class InteractiveInterface
{
  public EventResult HandleEvent(Event e)
  {
    LogMessage($"{e}");

    if (
      e is KeyEvent
      {
        Key: Key.Character,
        Char.IsAscii: true,
        Char.Value: 'C',
        Modifiers: ModifierKey.Ctrl
      }
    )
    {
      return EventResult.Exit;
    }
    else if (
      e is KeyEvent
      {
        Key: Key.Character,
        Char.IsAscii: true,
        Char.Value: 'D',
        Modifiers: ModifierKey.Ctrl
      }
    )
    {
      state.showDebugger = true;
    }
    else if (e is KeyEvent { Key: Key.Backspace })
    {
      state.currentCommandLine = state.currentCommandLine.Substring(
        0,
        Math.Max(state.currentCommandLine.Length - 1, 0)
      );
    }
    else if (
      e is KeyEvent
      {
        Key: Key.Character,
        Char.IsAscii: true,
        Char.Value: 'M',
        Modifiers: ModifierKey.Ctrl
      }
    )
    {
      InterpretState();
      return EventResult.Interpret;
    }
    else if (e is KeyEvent { Key: Key.Character, Char.IsAscii: true } ke)
    {
      state.currentCommandLine += ke.Char.ToString();
    }
    return EventResult.Redraw;
  }
}
