public class TypeCahce
{
  public static Task<Dictionary<string, Type>> Cache = Task.Run(() =>
  {
    var TypeCahce = new Dictionary<string, Type>();
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
      foreach (var type in assembly.GetTypes())
      {
        var name = type.FullName ?? "";
        if (TypeCahce.ContainsKey(name))
        {
          TypeCahce[name] = null!;
        }
        else
        {
          TypeCahce[name] = type;
        }
      }
    }
    return TypeCahce;
  });
}
