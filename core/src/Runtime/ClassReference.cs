using System.Reflection;
using CriusNyx.Util;

public class ClassReference(Type type) : IDeref
{
  public Type Type => type;

  public object Deref(string memberName)
  {
    return new MethodGroupReference(
      null!,
      Type.GetMember(memberName).WhereAs<MethodInfo>().ToArray()
    );
  }
}
