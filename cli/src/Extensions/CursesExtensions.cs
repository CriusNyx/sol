using Sharpie.Abstractions;

namespace Sol.CLI;

public static class CursesExtensions
{
  public static void SplitH(
    this IScreen source,
    int at,
    Action<IWindow> left,
    Action<IWindow> right
  )
  {
    var size = source.Size;

    left(source.Window(new(0, 0, at, source.Size.Height)));
    right(source.Window(new(at, 0, size.Width - at, size.Height)));
  }

  public static void SplitV(
    this IScreen source,
    int at,
    Action<IWindow> top,
    Action<IWindow> bottom
  )
  {
    var size = source.Size;
    top(source.Window(new(0, 0, size.Width, at)));
    bottom(source.Window(new(0, at, size.Width, size.Height - at)));
  }

  public static void Padding(this IScreen source, int padding, Action<IWindow> window)
  {
    var size = source.Size;
    window(
      source.Window(new(padding, padding, size.Width - padding * 2, size.Height - padding * 2))
    );
  }
}
