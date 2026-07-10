using CriusNyx.Util;

namespace DevCon.TypeSystem;

/// <summary>
/// Scope which contains global types.
/// </summary>
public class GlobalScope : TypeScope
{
  private static IDictionary<string, IDictionary<string, DevConType>> GenerateNamespaceCache()
  {
    var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.ExportedTypes);

    var namespaces = new Dictionary<string, IDictionary<string, DevConType>>();
    foreach (var type in types)
    {
      var ns = type.Namespace ?? "";
      var nsTypes = namespaces.GetOrSet(ns, () => new Dictionary<string, DevConType>());
      var typeName = type.Name;
      if (nsTypes.ContainsKey(typeName))
      {
        nsTypes[typeName] = new AmbiguousType();
      }
      else
      {
        nsTypes[typeName] = new ClassReferenceType(type);
      }
    }

    return namespaces;
  }

  static IDictionary<string, IDictionary<string, DevConType>> NamespaceCahce =
    GenerateNamespaceCache();
  static IDictionary<string, DevConType> globalTypes =
    NamespaceCahce.Safe("") ?? new Dictionary<string, DevConType>();

  /// <summary>
  /// Create a new empty global scope.
  /// </summary>
  public GlobalScope() { }

  /// <summary>
  /// Do not use.
  /// </summary>
  /// <param name="name"></param>
  /// <param name="type"></param>
  /// <exception cref="InvalidOperationException"></exception>
  public override void SetType(string name, DevConType type)
  {
    throw new InvalidOperationException("Cannot set global types");
  }

  /// <summary>
  /// Get global type.
  /// </summary>
  /// <param name="name"></param>
  /// <returns></returns>
  public override DevConType? GetType(string name)
  {
    {
      if (globalTypes.Safe(name) is DevConType type)
      {
        return type;
      }
    }
    return null;
  }

  /// <summary>
  /// Get the namespace at the specified path.
  /// </summary>
  /// <param name="namespacePath"></param>
  /// <returns></returns>
  public IDictionary<string, DevConType> GetNamespace(string namespacePath)
  {
    return NamespaceCahce.Safe(namespacePath) ?? new Dictionary<string, DevConType>();
  }
}
