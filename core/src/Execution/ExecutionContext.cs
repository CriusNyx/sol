using DevCon.Runtime;
using DevCon.TypeSystem;

namespace DevCon.Execution;

/// <summary>
/// The context used to evaluate programs.
/// </summary>
public class ExecutionContext
{
  /// <summary>
  /// The set of namespaces included currently.
  /// </summary>
  List<string> usings = new List<string>();

  // TODO: Refactor values to incorporate scopes and namespaces.

  /// <summary>
  /// The current set of variables in memory.
  /// </summary>
  Dictionary<string, object> values = new Dictionary<string, object>();

  /// <summary>
  /// Include a namespace.
  /// </summary>
  /// <param name="ns"></param>
  public void UseNamespace(string ns)
  {
    usings.Add(ns);
  }

  /// <summary>
  /// Get the value for a variable by name.
  /// </summary>
  /// <param name="key"></param>
  /// <returns></returns>
  public object GetValue(string key)
  {
    if (values.TryGetValue(key, out var value))
    {
      return value;
    }
    else
    {
      var cache = TypeCahce.Cache.Result;
      if (cache.TryGetValue(key, out var result))
      {
        return new ClassReference(result);
      }
      // This looks wrong? This should be handled by scope.
      foreach (var ns in usings)
      {
        if (cache.TryGetValue($"{ns}.{key}", out result))
        {
          return new ClassReference(result);
        }
      }
    }

    return null!;
  }

  /// <summary>
  /// Set the value for a variable in scope.
  /// </summary>
  /// <param name="key"></param>
  /// <param name="value"></param>
  public void SetValue(string key, object value)
  {
    values[key] = value;
  }
}
