using System.Linq.Expressions;
using System.Reflection;

namespace DevCon.TypeSystem;

/// <summary>
/// Extension methods for working with class member values.
/// </summary>
public static class MemberInfoExtensions
{
  /// <summary>
  /// Is the member static.
  /// </summary>
  /// <param name="memberInfo"></param>
  /// <returns></returns>
  public static bool IsStatic(this MemberInfo memberInfo)
  {
    if (memberInfo is FieldInfo field)
    {
      return field.IsStatic;
    }
    if (memberInfo is PropertyInfo property)
    {
      return false;
    }
    if (memberInfo is MethodInfo method)
    {
      return method.IsStatic;
    }
    return false;
  }

  /// <summary>
  /// Create a delegate that can be invoked for a method.
  /// </summary>
  /// <param name="method"></param>
  /// <returns></returns>
  public static Delegate CreateMethodDelegate(this MethodInfo method)
  {
    var paramsTypes = method.GetParameters().Select(x => x.ParameterType);
    Type delegateType = Expression.GetDelegateType(paramsTypes.Append(method.ReturnType).ToArray());
    return Delegate.CreateDelegate(delegateType, method);
  }
}
