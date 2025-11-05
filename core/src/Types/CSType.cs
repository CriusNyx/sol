using CriusNyx.Util;
using Microsoft.VisualBasic;

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

  public ISet<Type> TypeSuperset()
  {
    HashSet<Type> superset = new HashSet<Type>();

    for (var t = csType; t != null; t = t.BaseType)
    {
      superset.Add(t);
    }

    csType.GetInterfaces().Select(superset.Add);

    return superset;
  }
}
