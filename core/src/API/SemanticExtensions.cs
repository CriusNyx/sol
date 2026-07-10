namespace DevCon.TypeSystem;

/// <summary>
///
/// </summary>
public static class SemanticExtensions
{
  /// <summary>
  /// Get semantic type for the specified DevConType
  /// </summary>
  /// <param name="devConType"></param>
  /// <returns></returns>
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
    else
    {
      return SemanticType.None;
    }
  }
}
