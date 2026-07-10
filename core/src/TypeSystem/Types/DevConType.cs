using System.Reflection;
using CriusNyx.Util;
using XDoc;

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

  public abstract IEnumerable<DocumentationElement> GetDocs();

  public IEnumerable<string> GetDocStrings()
  {
    return GetDocs()
      .Select(x =>
      {
        if (x is TypeDocumentation td)
        {
          return td.ToPlainText();
        }
        else if (x is MethodDocumentation md)
        {
          return md.ToPlainText();
        }
        return null!;
      })
      .WhereAs<string>();
  }
}
