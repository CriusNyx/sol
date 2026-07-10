namespace DevCon.AST;

/// <summary>
/// Base class for a right hand expression.
/// </summary>
public abstract class RightHandExpression : ASTNode
{
  // Doesn't really need an implementation because every language element can be evaluated.
  // This class really only exists for the parser combinator.
}
