namespace DevCon.TypeSystem;

public static class SemanticExtensions
{
  public static SemanticType ToSemanticType(this DevConType devConType)
  {
    if (devConType is ClassReferenceType)
    {
      return SemanticType.ClassName;
    }
    else if (devConType is NamespaceReference)
    {
      return SemanticType.ClassName;
    }
    else if (devConType is CSType)
    {
      return SemanticType.ObjectReference;
    }
    else if (devConType is InvocationType)
    {
      return SemanticType.MethodReference;
    }
    throw new NotImplementedException();
  }
}
