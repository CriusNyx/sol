using CriusNyx.Results;
using CriusNyx.Results.Extensions;
using CriusNyx.Util;
using DevCon.AST;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

/// <summary>
/// ASTNode for use statements.
/// </summary>
/// <param name="useKeyword"></param>
/// <param name="namespaceSequence"></param>
public class UseStatement(KeywordSpan? useKeyword, Identifier?[]? namespaceSequence) : ASTNode
{
  /// <summary>
  /// The KeywordSpan for the use keyword.
  /// </summary>
  public Option<KeywordSpan> UseKeyword => useKeyword.AsOption();

  /// <summary>
  /// The identifiers that comprise the namespace that this use statement includes.
  /// </summary>
  public Identifier?[]? NamespaceSequence => namespaceSequence;

  /// <summary>
  /// The identifier (including dots) of the namespace.
  /// </summary>
  public string NamespaceIdentifier =>
    namespaceSequence?.Select(x => x?.Source ?? "").StringJoin(".") ?? "";

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [(nameof(NamespaceSequence).With(NamespaceSequence!))];
  }

  public override object? Evaluate(ExecutionContext context)
  {
    context.UseNamespace(NamespaceIdentifier);
    return null;
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    context.typeScope.UseNamespace(NamespaceIdentifier);
    foreach (var ns in NamespaceSequence ?? [])
    {
      ns?.SetType(new NamespaceReference());
    }
    return new VoidType();
  }

  protected override Span _GetSpan()
  {
    return Span.SafeJoin(
      useKeyword?.GetSpan(),
      Span.SafeJoin(NamespaceSequence?.Select(x => x?.GetSpan()).ToArray() ?? [])
    );
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { useKeyword }
      .Concat(namespaceSequence ?? [])
      .WhereAs<ASTNode>();
  }
}
