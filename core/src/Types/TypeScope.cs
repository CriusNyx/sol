using CriusNyx.Util;

namespace DevCon.TypeSystem;

public class GlobalScope : TypeScope
{
  static Task<Dictionary<string, DevConType>> DevConTypeCache = Task.Run(async () =>
  {
    var result = await TypeCahce.Cache;
    return result
      .Select(
        (pair) =>
          ((string, DevConType))
            (
              pair.Key,
              pair.Value == null ? new AmbiguousType() : new ClassReferenceType(pair.Value)
            )
      )
      .ToDictionary();
  });

  public GlobalScope() { }

  public override void SetType(string name, DevConType type)
  {
    throw new InvalidOperationException("Cannot set global types");
  }

  public override DevConType? GetType(string name, IEnumerable<string>? usings = null)
  {
    {
      if (DevConTypeCache.Result.Safe(name) is DevConType type)
      {
        return type;
      }
    }
    foreach (var ns in usings ?? [])
    {
      if (DevConTypeCache.Result.Safe($"{ns}.{name}") is DevConType type)
      {
        return type;
      }
    }
    return null;
  }
}

public class TypeScope : DebugPrint
{
  TypeScope? parent;
  Dictionary<string, DevConType> values = new Dictionary<string, DevConType>();
  List<string> usings = new List<string>();

  public TypeScope(TypeScope? parent = null)
  {
    this.parent = parent;
  }

  public void UseNamespace(string ns)
  {
    usings.Add(ns);
  }

  public virtual void SetType(string name, DevConType type)
  {
    values[name] = type;
  }

  public virtual DevConType? GetType(string name, IEnumerable<string>? usings = null)
  {
    return values.Safe(name) ?? parent?.GetType(name, this.usings.Concat(usings ?? []));
  }

  public static TypeScope CreateGlobalScope()
  {
    return new GlobalScope().PushScope();
  }

  public TypeScope PushScope()
  {
    return new TypeScope(this);
  }

  public TypeScope? PopScope()
  {
    return parent;
  }

  public IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(parent).With(parent)!, nameof(values).With(values)];
  }
}
