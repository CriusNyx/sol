public class ExecutionContext
{
  List<string> usings = new List<string>();
  Dictionary<string, object> values = new Dictionary<string, object>();

  public void UseNamespace(string ns)
  {
    usings.Add(ns);
  }

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

  public void SetValue(string key, object value)
  {
    values[key] = value;
  }
}
