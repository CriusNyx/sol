namespace DevCon.Execution;

/// <summary>
/// Represents a value that can be dereferenced.
/// Used during execution to store references to values.
/// </summary>
public interface DerefableValue
{
  public object? Deref(string key);
}
