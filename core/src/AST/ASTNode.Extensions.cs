using CriusNyx.Util;

namespace DevCon.AST;

/// <summary>
/// Extensions for AST nodes.
/// </summary>
public static class ASTNodeExtensions
{
  /// <summary>
  /// Find a node with the specified predicate in the abstract syntax tree.
  /// </summary>
  /// <param name="astNode"></param>
  /// <param name="predicate"></param>
  /// <returns></returns>
  public static ASTNode? FindNode(this ASTNode astNode, Func<ASTNode, bool> predicate)
  {
    if (predicate(astNode))
    {
      return astNode;
    }
    foreach (var child in astNode.GetChildren())
    {
      if (child.FindNode(predicate) is ASTNode result)
      {
        return result;
      }
    }
    return null;
  }

  /// <summary>
  /// Format the node in a tree the additional information appended in a grid.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="formatters"></param>
  /// <returns></returns>
  public static string FormatWith(this ASTNode node, params Func<ASTNode, string>[] formatters)
  {
    string Indent(int level)
    {
      if (level <= 0)
      {
        return "";
      }
      else
      {
        return Enumerable.Repeat<string>("| ", level - 1).Concat(["|-"]).StringJoin();
      }
    }

    List<string[]> elements = new List<string[]>();
    foreach (var (child, level) in node.TraverseFlat(x => x?.GetChildren() ?? []))
    {
      var heading = Indent(level) + (child?.GetType().Name.ToString() ?? "null");
      var rest = formatters.Select(formatter => formatter(child!));
      elements.Add(heading.AsArray().Concat(rest).ToArray());
    }
    return elements.FormatGrid(" ");
  }

  /// <summary>
  /// Format the node in a tree with it's type information appended.
  /// </summary>
  /// <param name="node"></param>
  /// <returns></returns>
  public static string FormatWithTypes(this ASTNode node)
  {
    return node.FormatWith(
      (node) => node?.ShortCode() ?? "",
      (node) => node?.NodeTypeSafe?.ToString() ?? "null"
    );
  }
}
