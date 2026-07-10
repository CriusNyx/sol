namespace DevCon.Execution;

/// <summary>
/// Used to store references to values.
/// The reference can be dereferenced into a value, or assigned.
/// </summary>
public abstract class Reference
{
  public abstract Type GetRefType();
  public abstract object? Get();
  public abstract void Set(object? value);
}

/// <summary>
/// A reference to a CSharp object.
/// </summary>
/// <param name="owner"></param>
/// <param name="name"></param>
public class ObjectReference(object owner, string name) : Reference
{
  /// <summary>
  /// The CSharp object that owns the reference.
  /// </summary>
  public object Owner => owner;

  /// <summary>
  /// THe field or method name referenced.
  /// </summary>
  public string Name => name;

  /// <summary>
  /// Get the type of the owner of the reference.
  /// </summary>
  /// <returns></returns>
  public override Type GetRefType()
  {
    return Owner.GetType();
  }

  /// <summary>
  /// Dereference the reference into a value.
  /// </summary>
  /// <returns></returns>
  public override object? Get()
  {
    if (Owner is ExecutionContext context)
    {
      context.GetValue(Name);
    }
    return ExecutionHelpers.DerefMember(Owner, name);
  }

  /// <summary>
  /// Set the reference.
  /// </summary>
  /// <param name="value"></param>
  public override void Set(object? value)
  {
    if (Owner is ExecutionContext context)
    {
      context.SetValue(Name, value!);
    }
  }
}
