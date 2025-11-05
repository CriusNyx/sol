namespace Sol.TypeSystem;

public class CSType(Type type) : SolType
{
  public Type csType => type;

  public override SolType? DerefFieldType(string name)
  {
    var members = csType.GetMember(name).Where(x => !x.IsStatic()).ToArray();
    return From(members);
  }

  public override SolType MakeStatic()
  {
    return new ClassReferenceType(csType);
  }

  public override bool Equals(object? obj)
  {
    return obj is CSType type && EqualityComparer<Type>.Default.Equals(csType, type.csType);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(csType);
  }

  public override string ToString()
  {
    return $"{nameof(CSType)}({csType.Name})";
  }
}
