using CriusNyx.Util;

public static class CLI
{
  public static bool PromptYN(string message, string tryAgain = "Sorry, try again.")
  {
    for (int i = 0; i < 3; i++)
    {
      Console.WriteLine(message);
      var key = Console.ReadKey().Safe((x) => x.KeyChar);
      if (key == 'y')
      {
        return true;
      }
      if (key == 'n')
      {
        return false;
      }
      Console.WriteLine(tryAgain);
    }
    return false;
  }
}
