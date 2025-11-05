namespace Sol.TypeSystem;

public static class SemanticExtensions
{
  public static SemanticType ToSemanticType(this SolType solType)
  {
    if (solType is ClassReferenceType)
    {
      return SemanticType.ClassName;
    }
    else if (solType is NamespaceReference)
    {
      return SemanticType.ClassName;
    }
    else if (solType is CSType)
    {
      return SemanticType.ObjectReference;
    }
    else if (solType is InvocationType)
    {
      return SemanticType.MethodReference;
    }
    throw new NotImplementedException();
  }
}
