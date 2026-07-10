using DevCon.TypeSystem;
using XDoc;

public class UnknownType(DevConType? underlyingType) : DevConType
{
  public DevConType? UnderlyingType => underlyingType;

  public override string ToString()
  {
    return $"UnknownType({UnderlyingType?.ToString()})";
  }

  public override IEnumerable<DocumentationElement> GetDocs()
  {
    return UnderlyingType?.GetDocs() ?? [];
  }
}
