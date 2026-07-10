using System.Reflection;

namespace DevCon.Execution;

/// <summary>
/// Helper methods for program execution.
/// </summary>
public static class ExecutionHelpers
{
  /// <summary>
  /// Derefence a member from an object.
  /// </summary>
  /// <param name="owner"></param>
  /// <param name="fieldName"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static object? DerefMember(object owner, string fieldName)
  {
    if (owner.GetType().GetField(fieldName) is FieldInfo field)
    {
      return field.GetValue(owner);
    }
    else if (owner.GetType().GetProperty(fieldName) is PropertyInfo property)
    {
      return property.GetValue(owner);
    }
    else if (
      owner.GetType().GetMember(fieldName) is MemberInfo[] members
      && members.Length > 0
      && members.All(x => x is MethodInfo)
    )
    {
      new MethodGroupReference(owner, members.Select(x => x as MethodInfo).ToArray()!);
    }
    throw new InvalidOperationException();
  }

  /// <summary>
  /// Set object member.
  /// </summary>
  /// <param name="owner"></param>
  /// <param name="fieldName"></param>
  /// <param name="value"></param>
  /// <exception cref="InvalidOperationException"></exception>
  public static void SetMember(object owner, string fieldName, object? value)
  {
    if (owner.GetType().GetField(fieldName) is FieldInfo field)
    {
      field.SetValue(fieldName, value);
    }
    else if (owner.GetType().GetProperty(fieldName) is PropertyInfo property)
    {
      property.SetValue(fieldName, value);
    }
    throw new InvalidOperationException();
  }
}
