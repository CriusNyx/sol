using CriusNyx.Util;
using Sol.AST;
using Sol.DataStructures;
using Sol.TypeSystem;
using Superpower.Model;
using ExecutionContext = Sol.Execution.ExecutionContext;

public class SourceSpan(TextSpan source) : ASTNode
{
  public TextSpan Source => source;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Source).With(Source)];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    throw new NotImplementedException();
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [];
  }

  public override Span GetSpan()
  {
    return Source;
  }

  protected override SolType? _TypeCheck(TypeContext context)
  {
    throw new NotImplementedException();
  }

  public override string ShortCode()
  {
    return Source.ToString();
  }
}
