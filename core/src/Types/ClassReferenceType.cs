namespace DevCon.TypeSystem;

public class ClassReferenceType(Type type) : DevConType
{
  public Type classType => type;

  public override DevConType? DerefFieldType(string name)
  {
    var idk = type.GetMethods();
    var members = type.GetMember(name).Where(x => x.IsStatic()).ToArray();
    return From(members);
  }

  public override bool Equals(object? obj)
  {
    return obj is ClassReferenceType type
      && EqualityComparer<Type>.Default.Equals(classType, type.classType);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(classType);
  }

  public override string ToString()
  {
    return $"{nameof(ClassReferenceType)}({classType.Name})";
  }
}
