namespace DevCon.Tests;

public class ProgressiveTypingTest
{
  [DatapointSource]
  public string[] Paths => Directory.GetFiles("testPrograms", "*.dcn");

  [Theory]
  public void ProgramCompilesWhileBeingTyped(string path)
  {
    var fileText = File.ReadAllText(path);

    foreach (var i in Enumerable.Range(0, fileText.Length))
    {
      var source = fileText.Substring(0, i);

      Assert.DoesNotThrow(() =>
      {
        var compiledResult = Compiler.Parse(source);
      });
    }
  }
}
