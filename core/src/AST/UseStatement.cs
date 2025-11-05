using CriusNyx.Util;
using Sol.AST;
using Sol.DataStructures;
using Sol.TypeSystem;
using ExecutionContext = Sol.Execution.ExecutionContext;

public class UseStatement(KeywordSpan useKeyword, Identifier[] namespaceSequence) : ASTNode
{
  public KeywordSpan UseKeyword => useKeyword;
  public Identifier[] NamespaceSequence => namespaceSequence;
  public string NamespaceIdentifier =>
    NamespaceSequence?.Select(x => x.Source).StringJoin(".") ?? "";

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [(nameof(NamespaceSequence).With(NamespaceSequence))];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    context.UseNamespace(NamespaceIdentifier);
    return null;
  }

  protected override SolType? _TypeCheck(TypeContext context)
  {
    context.typeScope.UseNamespace(NamespaceIdentifier);
    foreach (var ns in NamespaceSequence)
    {
      ns.SetType(new NamespaceReference());
    }
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
