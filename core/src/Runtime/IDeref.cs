namespace DevCon.Runtime;

/// <summary>
/// A value that can be derefenced with a string.
/// </summary>
public interface IDeref
{
  /// <summary>
  /// Derferenced the member from this value.
  /// </summary>
  /// <param name="memberName"></param>
  /// <returns></returns>
  public object Deref(string memberName);
}
