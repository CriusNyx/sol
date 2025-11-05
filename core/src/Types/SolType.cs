using System.Reflection;
using CriusNyx.Util;

namespace Sol.TypeSystem;

public abstract class SolType
{
  public virtual SolType? DerefFieldType(string name)
  {
    throw new NotImplementedException();
  }

  public virtual SolType? DerefIndexType()
  {
    throw new NotImplementedException();
  }

  public virtual SolType? DerefReturnType(SolType[] knownArgumentTypes)
  {
    throw new NotImplementedException();
  }

  public virtual SolType MakeStatic()
  {
    throw new NotImplementedException();
  }

  public static SolType? From(MemberInfo[] members)
  {
    if (members.Length == 1 && members.First() is MemberInfo first)
    {
      if (first is FieldInfo field)
      {
        return new CSType(field.FieldType.NotNull());
      }
      else if (first is PropertyInfo property)
      {
        return new CSType(property.PropertyType.NotNull());
      }
    }
    if (members.Count() > 0 && members.All(x => x is MethodInfo))
    {
      return new InvocationType(members.Select(x => x as MethodInfo).ToArray()!);
    }
    return null;
  }

  public override string ToString()
  {
    return GetType().Name.ToString();
  }
}
