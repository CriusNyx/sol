using CriusNyx.Util;

namespace Sol.AST;

public class Assign(LeftHandExpression left, RightHandExpression right) : ASTNode
{
  public LeftHandExpression Left => left;
  public RightHandExpression Right => right;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Left).With(Left), nameof(Right).With(Right)];
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    var rightType = right.TypeCheck(context).NotNull();
    if (left.GetLocalName() is string localName)
    {
      context.typeScope.SetType(localName, rightType);
    }
    var leftType = left.TypeCheck(context).NotNull();
    return rightType;
  }

  public override object? Evaluate(ExecutionContext context)
  {
    var reference = Left.EvaluateReference(context);
    var value = Right.Evaluate(context);
    reference.Set(value);
    return null;
  }
}
