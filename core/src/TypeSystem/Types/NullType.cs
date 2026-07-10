using XDoc;

namespace DevCon.TypeSystem;

public class NullType : DevConType
{
  public override IEnumerable<DocumentationElement> GetDocs()
  {
    return [];
  }
}
