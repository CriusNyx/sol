using System.ComponentModel;
using CriusNyx.Util;
using Microsoft.VisualBasic;

public class GlobalScope : TypeScope
{
  static Task<Dictionary<string, SolType>> SolTypeCache = Task.Run(async () =>
  {
    var result = await TypeCahce.Cache;
    return result
      .Select(
        (pair) =>
          ((string, SolType))
            (
              pair.Key,
              pair.Value == null ? new AmbiguousType() : new ClassReferenceType(pair.Value)
            )
      )
      .ToDictionary();
  });

  public GlobalScope() { }

  public override void SetType(string name, SolType type)
  {
    throw new InvalidOperationException("Cannot set global types");
  }

  public override SolType? GetType(string name, IEnumerable<string>? usings = null)
  {
    {
      if (SolTypeCache.Result.Safe(name) is SolType type)
      {
        return type;
      }
    }
    foreach (var ns in usings ?? [])
    {
      if (SolTypeCache.Result.Safe($"{ns}.{name}") is SolType type)
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
  Dictionary<string, SolType> values = new Dictionary<string, SolType>();
  List<string> usings = new List<string>();

  public TypeScope(TypeScope? parent = null)
  {
    this.parent = parent;
  }

  public void UseNamespace(string ns)
  {
    usings.Add(ns);
  }

  public virtual void SetType(string name, SolType type)
  {
    values[name] = type;
  }

  public virtual SolType? GetType(string name, IEnumerable<string>? usings = null)
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
