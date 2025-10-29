using System.Reflection;
using CriusNyx.Util;

public static class MethodHelpers
{
  public static object? DynamicInvoke(object source, string methodName, object[] arguments)
  {
    var method = BindMethod(
      source.GetType(),
      methodName,
      arguments.Select(x => x.GetType()).ToArray()
    );
    return method?.Invoke(source, arguments);
  }

  public static object? DynamicInvoke(object source, MethodInfo[] overloads, object[] arguments)
  {
    var method = BindMethod(overloads, arguments.Select(x => x.GetType()).ToArray());
    return method?.Invoke(source, arguments);
  }

  public static MethodInfo? BindMethod(Type sourceType, string methodName, Type[] argumentTypes)
  {
    return BindMethod(
      sourceType.GetMember(methodName).WhereAs<MethodInfo>().ToArray(),
      argumentTypes
    );
  }

  public static MethodInfo? BindMethod(MethodInfo[] overloads, Type[] argumentTypes)
  {
    return Type.DefaultBinder.SelectMethod(BindingFlags.Default, overloads, argumentTypes, null)
      as MethodInfo;
  }
}
