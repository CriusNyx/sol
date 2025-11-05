namespace Sol.Execution;

public interface DerefableValue
{
  public object? Deref(string key);
}
