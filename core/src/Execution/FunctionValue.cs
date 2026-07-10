using System.Reflection;

namespace DevCon.Execution;

/// <summary>
/// Used to store and pass functions.
/// </summary>
public interface FunctionValue
{
  object Invoke(object[] values);
}

// TODO: By the time type checking is complete the MethodInfo to invoke should be known.

/// <summary>
/// A reference to a CS method group.
/// </summary>
public class MethodGroupReference(object owner, MethodInfo[] overloads) : FunctionValue
{
  /// <summary>
  /// The object that owns this method.
  /// Will be passed to the this value in the CSharp method.
  /// </summary>
  public object Owner => owner;

  /// <summary>
  /// Set of method overloads that might be invoked.
  /// </summary>
  public MethodInfo[] Overloads => overloads;

  /// <summary>
  /// Invoke the method with the arguments provided.
  /// </summary>
  /// <param name="values"></param>
  /// <returns></returns>
  public object Invoke(object[] values)
  {
    return MethodHelpers.DynamicInvoke(Owner, Overloads, values)!;
  }
}
