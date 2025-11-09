using System.Reflection;
using CriusNyx.Util;

namespace DevCon.TypeSystem;

public abstract class DevConType
{
  public virtual DevConType? DerefFieldType(string name)
  {
    throw new NotImplementedException();
  }

  public virtual DevConType? DerefIndexType()
  {
    throw new NotImplementedException();
  }

  public virtual DevConType? DerefReturnType(DevConType[] knownArgumentTypes)
  {
    throw new NotImplementedException();
  }

  public virtual DevConType MakeStatic()
  {
    throw new NotImplementedException();
  }

  public static DevConType? From(MemberInfo[] members)
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
