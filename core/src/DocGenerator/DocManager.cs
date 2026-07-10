using System.Reflection;
using CriusNyx.Results;
using CriusNyx.Results.Extensions;
using CriusNyx.Util;
using Microsoft.CodeAnalysis;
using XDoc;

// using XDoc;

namespace DevCon.Docs;

public class DocManager
{
  const string DotnetExeName = "dotnet";
  const string PackFolderName = "packs";
  const string RefFolderName = "ref";

  public static XDoc.XDoc xdocInstance { get; private set; }
  private static Dictionary<string, string[]> refAssemblyCache;

  static DocManager()
  {
    refAssemblyCache = GetRefAssemblyCache();

    var configuration = new XDocConfiguration { GetXmlDocumentationFilePath = ResolveAssemblyDocs };

    xdocInstance = new XDoc.XDoc(configuration);
  }

  private static string ResolveAssemblyDocs(Assembly assembly)
  {
    var assemblyFolder = Path.GetFullPath(Path.Join(assembly.Location, ".."));
    var docFileName = $"{Path.GetFileNameWithoutExtension(assembly.Location)}.xml";
    var libraryDocsFilePath = Path.Join(assemblyFolder, docFileName);

    if (File.Exists(libraryDocsFilePath))
    {
      return libraryDocsFilePath;
    }

    return refAssemblyCache?.Safe(docFileName)?.FirstOrDefault()!;
  }

  public static Option<TypeDocumentation> GetClassDoc(Type type)
  {
    return xdocInstance.Get(type).AsOption();
  }

  public static IEnumerable<MethodDocumentation> GetMethodDocs(Type type, string methodName)
  {
    return GetMethodDocs(type.GetMethods().Where(x => x.Name == methodName));
  }

  public static IEnumerable<MethodDocumentation> GetMethodDocs(IEnumerable<MethodInfo> methodInfo)
  {
    return methodInfo.Select(x => xdocInstance.Get(x)).WhereAs<MethodDocumentation>();
  }

  private static IEnumerable<string> GetDotnetLocations()
  {
    var canidates = Environment.GetEnvironmentVariable("PATH")?.Split(":") ?? [];

    return canidates.Where(x => File.Exists(Path.Join(x, DotnetExeName)));
  }

  private static IEnumerable<string> GetPacksPaths()
  {
    foreach (var dotnetLoc in GetDotnetLocations())
    {
      var packsDir = Path.Join(dotnetLoc, PackFolderName);
      if (Directory.Exists(packsDir))
      {
        foreach (var pack in Directory.GetDirectories(packsDir))
        {
          yield return pack;
        }
      }
    }
  }

  private static Option<string> GetRefAssemblyFolderFromPack(string packDir)
  {
    var dotnetVersion = Environment.Version;
    var major = dotnetVersion.Major;
    var minor = dotnetVersion.Minor;
    string netFolderName = $"net{major}.{minor}";
    var assemblyFolderPath = Path.Join(
      packDir,
      dotnetVersion.ToString(),
      RefFolderName,
      netFolderName
    );
    if (Directory.Exists(assemblyFolderPath))
    {
      return Option.Some(assemblyFolderPath);
    }
    return Option.None<string>();
  }

  private static Dictionary<string, string[]> GetRefAssemblyCache()
  {
    var assemblyFolders = GetPacksPaths().Select(GetRefAssemblyFolderFromPack).WhereSome();
    var xmlFiles = assemblyFolders.SelectMany(x => Directory.GetFiles(x, "*.xml"));
    return xmlFiles.GroupBy(Path.GetFileName).ToDictionary(x => x.Key ?? "", x => x.ToArray());
  }
}
