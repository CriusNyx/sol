using System.Reflection;
using CriusNyx.Util;

/// <summary>
/// Helper methods for working with CSharp methods.
/// </summary>
public static class MethodHelpers
{
  /// <summary>
  /// Dynamically invoke CSharp method.
  /// </summary>
  /// <param name="source"></param>
  /// <param name="methodName"></param>
  /// <param name="arguments"></param>
  /// <returns></returns>
  public static object? DynamicInvoke(object source, string methodName, object[] arguments)
  {
    var method = BindMethod(
      source.GetType(),
      methodName,
      arguments.Select(x => x.GetType()).ToArray()
    );
    return method?.Invoke(source, arguments);
  }

  /// <summary>
  /// Dynamically invoke CSharp method.
  /// </summary>
  /// <param name="source"></param>
  /// <param name="overloads"></param>
  /// <param name="arguments"></param>
  /// <returns></returns>
  public static object? DynamicInvoke(object source, MethodInfo[] overloads, object[] arguments)
  {
    var method = BindMethod(overloads, arguments.Select(x => x.GetType()).ToArray());
    return method?.Invoke(source, arguments);
  }

  /// <summary>
  /// Determine which MethodInfo to bind given the argument types.
  /// </summary>
  /// <param name="sourceType"></param>
  /// <param name="methodName"></param>
  /// <param name="argumentTypes"></param>
  /// <returns></returns>
  public static MethodInfo? BindMethod(Type sourceType, string methodName, Type[] argumentTypes)
  {
    return BindMethod(
      sourceType.GetMember(methodName).WhereAs<MethodInfo>().ToArray(),
      argumentTypes
    );
  }

  /// <summary>
  /// Determine which MethodInfo to bind given the argument types.
  /// </summary>
  /// <param name="overloads"></param>
  /// <param name="argumentTypes"></param>
  /// <returns></returns>
  public static MethodInfo? BindMethod(MethodInfo[] overloads, Type[] argumentTypes)
  {
    if (argumentTypes == null)
    {
      return null;
    }
    return Type.DefaultBinder.SelectMethod(BindingFlags.Default, overloads, argumentTypes, null)
      as MethodInfo;
  }
}
