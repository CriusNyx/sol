using CriusNyx.Util;
using Sol.AST;
using Superpower.Model;

public class UseStatement(SourceSpan useKeyword, Identifier[] namespaceSequence) : ASTNode
{
  public SourceSpan UseKeyword => useKeyword;
  public Identifier[] NamespaceSequence => namespaceSequence;
  public string NamespaceIdentifier => NamespaceSequence.Select(x => x.Source).StringJoin(".");

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [(nameof(NamespaceSequence).With(NamespaceSequence))];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    context.UseNamespace(NamespaceIdentifier);
    return null;
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    context.typeScope.UseNamespace(NamespaceIdentifier);
    return new VoidType();
  }

  public override Span GetSpan()
  {
    return Span.Join(
      useKeyword.GetSpan(),
      Span.Join(namespaceSequence.Select(x => x.GetSpan()).ToArray())
    );
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    yield return useKeyword;
    foreach (var ns in namespaceSequence)
    {
      yield return ns;
    }
  }
}
