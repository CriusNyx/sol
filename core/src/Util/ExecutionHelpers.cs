using System.Reflection;

namespace Sol.Execution;

public static class ExecutionHelpers
{
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
