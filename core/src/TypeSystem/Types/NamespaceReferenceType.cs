using DevCon.TypeSystem;
using XDoc;

/// <summary>
/// Type for a namespace reference.
/// </summary>
public class NamespaceReference : DevConType
{
  public override IEnumerable<DocumentationElement> GetDocs()
  {
    return [];
  }
}
