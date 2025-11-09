using System.Linq.Expressions;
using System.Reflection;

namespace DevCon.TypeSystem;

static class MemberInfoExtensions
{
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

  public static Delegate CreateDelegate(this MethodInfo method)
  {
    var paramsTypes = method.GetParameters().Select(x => x.ParameterType);
    Type delegateType = Expression.GetDelegateType(paramsTypes.Append(method.ReturnType).ToArray());
    return Delegate.CreateDelegate(delegateType, method);
  }
}
