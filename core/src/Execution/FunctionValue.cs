using System.Reflection;

public interface FunctionValue
{
  object Invoke(object[] values);
}

/// <summary>
/// A reference to a CS method group.
/// </summary>
public class MethodGroupReference(object owner, MethodInfo[] overloads) : FunctionValue
{
  public object Owner => owner;
  public MethodInfo[] Overloads => overloads;

  public object Invoke(object[] values)
  {
    return MethodHelpers.DynamicInvoke(Owner, Overloads, values)!;
  }
}
