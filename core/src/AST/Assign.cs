using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

public class Assign(LeftHandExpression? left, SourceSpan? equal, RightHandExpression? right)
  : ASTNode
{
  public LeftHandExpression? Left => left;
  public SourceSpan? Equal => equal;
  public RightHandExpression? Right => right;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Left).With(Left)!, nameof(Right).With(Right)!];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    var rightType = right?.TypeCheck(context) ?? new UnknownType();
    if (left?.GetLocalName() is string localName)
    {
      context.typeScope.SetType(localName, rightType);
    }
    var leftType = left?.TypeCheck(context) ?? new UnknownType();
    return rightType;
  }

  public override object? Evaluate(ExecutionContext context)
  {
    var reference = Left.NotNull().EvaluateReference(context);
    var value = Right.NotNull().Evaluate(context);
    reference.Set(value);
    return null;
  }

  public override Span GetSpan()
  {
    return Span.SafeJoin(Left?.GetSpan(), Equal?.GetSpan(), Right?.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { Left, Equal, Right }.WhereAs<ASTNode>();
  }
}
