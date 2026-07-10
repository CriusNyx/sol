using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// AST node for assign statement.
/// </summary>
/// <param name="left"></param>
/// <param name="equal"></param>
/// <param name="right"></param>
public class Assign(LeftHandExpression? left, SourceSpan? equal, RightHandExpression? right)
  : ASTNode
{
  /// <summary>
  /// The left hand side of the assign statement.
  /// </summary>
  public LeftHandExpression? Left => left;

  /// <summary>
  /// The SourceSpan for the equal expression.
  /// </summary>
  public SourceSpan? Equal => equal;

  /// <summary>
  /// The right hand side of the assign statement.
  /// </summary>
  public RightHandExpression? Right => right;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Left).With(Left)!, nameof(Right).With(Right)!];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    var rightType = right?.TypeCheck(context) ?? new UnknownType(null);
    if (left?.GetLocalName() is string localName)
    {
      context.typeScope.SetType(localName, rightType);
    }
    var leftType = left?.TypeCheck(context) ?? new UnknownType(null);
    return rightType;
  }

  public override object? Evaluate(ExecutionContext context)
  {
    var reference = Left.NotNull().EvaluateReference(context);
    var value = Right.NotNull().Evaluate(context);
    reference?.Set(value);
    return null;
  }

  protected override Span _GetSpan()
  {
    return Span.SafeJoin(Left?.GetSpan(), Equal?.GetSpan(), Right?.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { Left, Equal, Right }.WhereAs<ASTNode>();
  }
}
