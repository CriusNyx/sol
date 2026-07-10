using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// The binary operator type.
/// </summary>
public enum BinaryOpType
{
  Add,
  Subtract,
  Multiply,
  Divide,
  Modulo,
}

/// <summary>
/// ASTNode for a binary operation.
/// </summary>
/// <param name="opSource"></param>
/// <param name="type"></param>
/// <param name="left"></param>
/// <param name="right"></param>
public class BinaryOp(
  SourceSpan opSource,
  BinaryOpType type,
  RightHandExpression left,
  RightHandExpression right
) : RightHandExpression
{
  /// <summary>
  /// The source span for the operator.
  /// </summary>
  public SourceSpan OpSource => opSource;

  /// <summary>
  /// The type for the operator.
  /// </summary>
  public BinaryOpType Type => type;

  /// <summary>
  /// The left hand side of the operator.
  /// </summary>
  public RightHandExpression Left => left;

  /// <summary>
  /// The right hand side of the operator.
  /// </summary>
  public RightHandExpression Right => right;

  /// <summary>
  /// Used to dereference CSharp operators for the type.
  /// </summary>
  static Dictionary<BinaryOpType, string> CSMethodNames = new Dictionary<BinaryOpType, string>
  {
    { BinaryOpType.Add, "op_Addition" },
    { BinaryOpType.Subtract, "op_Subtraction" },
    { BinaryOpType.Multiply, "op_Multiply" },
    { BinaryOpType.Divide, "op_Division" },
    { BinaryOpType.Modulo, "op_Modulus" },
  };

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Type).With(Type), nameof(Left).With(Left), nameof(Right).With(Right)];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    var opMethodName = CSMethodNames[Type];

    var leftType = Left.TypeCheck(context).NotNull();
    var rightType = Right?.TypeCheck(context) ?? new UnknownType(null);
    var opMethod = leftType.MakeStatic().DerefFieldType(opMethodName);
    if (opMethod is DevConType devConType)
    {
      return devConType.DerefReturnType([leftType, rightType]);
    }
    return leftType;
  }

  public override object Evaluate(ExecutionContext context)
  {
    dynamic left = Left.Evaluate(context)!;
    dynamic right = Right.Evaluate(context)!;
    switch (Type)
    {
      case BinaryOpType.Add:
        return left + right;
      case BinaryOpType.Subtract:
        return left - right;
      case BinaryOpType.Multiply:
        return left * right;
      case BinaryOpType.Divide:
        return left / right;
      case BinaryOpType.Modulo:
        return left % right;
      default:
        throw new NotImplementedException();
    }
  }

  protected override Span _GetSpan()
  {
    return Span.Join(OpSource.GetSpan(), Left.GetSpan(), Right.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [Left, OpSource, Right];
  }
}
