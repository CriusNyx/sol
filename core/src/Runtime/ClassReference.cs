using System.Reflection;
using CriusNyx.Util;
using DevCon.Execution;

namespace DevCon.Runtime;

/// <summary>
/// A reference to a class.
/// </summary>
/// <param name="type"></param>
public class ClassReference(Type type) : IDeref
{
  /// <summary>
  /// the class referenced.
  /// </summary>
  public Type Type => type;

  /// <summary>
  /// Dereference the type.
  /// </summary>
  /// <param name="memberName"></param>
  /// <returns></returns>
  public object Deref(string memberName)
  {
    return new MethodGroupReference(
      null!,
      Type.GetMember(memberName).WhereAs<MethodInfo>().ToArray()
    );
  }
}
