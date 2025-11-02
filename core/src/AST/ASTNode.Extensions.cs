namespace Sol.AST;

public static class ASTNodeExtensions
{
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
}
