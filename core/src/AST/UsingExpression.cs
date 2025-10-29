using CriusNyx.Util;
using Sol.AST;

public class UseExpression(Identifier[] namespaceSequence) : ASTNode
{
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

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    context.typeScope.UseNamespace(NamespaceIdentifier);
    return new VoidType();
  }
}
