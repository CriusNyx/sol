using CriusNyx.Util;
using Sol.Parser;

var context = new ExecutionContext();

string[] sampleProgram = ["use System"];

foreach (var line in sampleProgram)
{
  Interpret(line);
}

while (Console.ReadLine() is string line)
{
  Interpret(line);
}

void Interpret(string line)
{
  var programLine = SolParser.Parse(line);
  var result = programLine.Evaluate(context);
  if (result != null)
  {
    Console.WriteLine(result.Debug());
  }
}

class TestClass
{
  public string field { get; set; } = null!;

  public string Foo(string value)
  {
    return $"value: {value}";
  }
}
