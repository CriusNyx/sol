using CriusNyx.Util;

/// <summary>
/// Helper methods for resolving paths.
/// </summary>
public static class PathHelpers
{
  /// <summary>
  /// Locate the file with the filename on the machine.
  /// </summary>
  /// <param name="filename"></param>
  /// <returns></returns>
  public static string[] Which(string filename)
  {
    List<string> canidates = new List<string>();
    var paths = Environment.GetEnvironmentVariable("PATH").OrDefault("").Split(":");
    foreach (var path in paths)
    {
      var canidatePath = Path.Join(path, filename);
      if (File.Exists(canidatePath))
      {
        canidates.Add(canidatePath);
      }
    }
    return canidates.ToArray();
  }
}
