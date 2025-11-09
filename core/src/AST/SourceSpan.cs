using CriusNyx.Util;
using DevCon.AST;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using Superpower.Model;
using ExecutionContext = DevCon.Execution.ExecutionContext;

public class SourceSpan(Span span, string source) : ASTNode
{
  public Span Span => span;
  public string Source => source;

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
    return span;
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    throw new NotImplementedException();
  }

  public override string ShortCode()
  {
    return Source.ToString();
  }

  public static implicit operator SourceSpan(TextSpan textSpan)
  {
    return new SourceSpan(
      textSpan,
      textSpan.Source.NotNull().Substring(textSpan.Position.Absolute, textSpan.Length)
    );
  }

  public override ASTNode? GetNodeUnderCursor(int position)
  {
    return null;
  }
}
