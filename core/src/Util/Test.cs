public static class TestExtension
{
  public static bool Try<R>(this Func<R> function, out R result)
  {
    try
    {
      result = function();
      return true;
    }
    catch
    {
      result = default!;
      return false;
    }
  }
}
