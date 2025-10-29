using CriusNyx.Util;
using Superpower.Model;

namespace Sol.AST;

public class Identifier(TextSpan textSpan) : ASTNode
{
  public TextSpan Span => textSpan;
  public string Source => Span.ToString();

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Span).With(Span)];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    throw new InvalidOperationException();
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    throw new NotImplementedException();
  }
}
