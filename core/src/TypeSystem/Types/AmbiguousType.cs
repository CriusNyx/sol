using XDoc;

namespace DevCon.TypeSystem;

public class AmbiguousType : DevConType
{
  public override IEnumerable<DocumentationElement> GetDocs()
  {
    return [];
  }
}
