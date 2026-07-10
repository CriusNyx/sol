using System.Security.Cryptography.X509Certificates;
using CriusNyx.Util;

namespace DevCon.TypeSystem;

/// <summary>
/// Scoped type information.
/// </summary>
public class TypeScope : DebugPrint
{
  /// <summary>
  /// Parent scope.
  /// </summary>
  TypeScope? parent;

  /// <summary>
  /// Variables in the scope.
  /// </summary>
  Dictionary<string, DevConType> values = new Dictionary<string, DevConType>();

  /// <summary>
  /// Create a new type scope.
  /// </summary>
  /// <param name="parent"></param>
  public TypeScope(TypeScope? parent = null)
  {
    this.parent = parent;
    Init();
  }

  /// <summary>
  /// Initialize type scope.
  /// </summary>
  private void Init()
  {
    foreach (var use in globalUsings)
    {
      UseNamespace(use);
    }
  }

  /// <summary>
  /// Get the global scope.
  /// </summary>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  private GlobalScope GetGlobalScope()
  {
    if (this is GlobalScope globalScope)
    {
      return globalScope;
    }
    return parent?.GetGlobalScope() ?? throw new InvalidOperationException();
  }

  /// <summary>
  /// Use namespace in this scope.
  /// </summary>
  /// <param name="namespacePath"></param>
  public void UseNamespace(string namespacePath)
  {
    var global = GetGlobalScope();
    foreach (var (name, type) in global.GetNamespace(namespacePath))
    {
      SetType(name, type);
    }
  }

  /// <summary>
  /// Set the type in this scope.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="type"></param>
  public virtual void SetType(string name, DevConType type)
  {
    values[name] = type;
  }

  /// <summary>
  /// Get the type from this scope.
  /// </summary>
  /// <param name="name"></param>
  /// <returns></returns>
  public virtual DevConType? GetType(string name)
  {
    return values.Safe(name) ?? parent?.GetType(name);
  }

  /// <summary>
  /// Create global scope
  /// </summary>
  /// <returns></returns>
  public static TypeScope CreateGlobalScope()
  {
    return new GlobalScope().PushScope();
  }

  /// <summary>
  /// Embed this scope inside a new scope.
  /// </summary>
  /// <returns></returns>
  public TypeScope PushScope()
  {
    return new TypeScope(this);
  }

  /// <summary>
  /// Return the scope inside this one.
  /// </summary>
  /// <returns></returns>
  public TypeScope? PopScope()
  {
    return parent;
  }

  /// <summary>
  /// For .Debug method
  /// </summary>
  /// <returns></returns>
  public IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(parent).With(parent)!, nameof(values).With(values)];
  }

  // ================================ Use Global ================================

  private static List<string> globalUsings = new List<string>();

  /// <summary>
  /// Use a namespace globaly in all DevCon programs.
  /// </summary>
  /// <param name="namespacePath"></param>
  public static void UseGlobal(string namespacePath)
  {
    globalUsings.Add(namespacePath);
  }
}
