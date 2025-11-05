namespace Sol.Execution;

public abstract class Reference
{
  public abstract Type GetRefType();
  public abstract object? Get();
  public abstract void Set(object? value);
}

public class ObjectReference(object owner, string name) : Reference
{
  public object Owner => owner;
  public string Name => name;

  public override Type GetRefType()
  {
    return Owner.GetType();
  }

  public override object? Get()
  {
    if (Owner is ExecutionContext context)
    {
      context.GetValue(Name);
    }
    return ExecutionHelpers.DerefMember(Owner, name);
  }

  public override void Set(object? value)
  {
    if (Owner is ExecutionContext context)
    {
      context.SetValue(Name, value!);
    }
  }
}
