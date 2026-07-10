using XDoc;

namespace DevCon.TypeSystem;

public class VoidType : DevConType
{
  public override IEnumerable<DocumentationElement> GetDocs()
  {
    return [];
  }
}
