using System.Reflection;
using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// The operator for the unary op.
/// </summary>
public enum UnaryOpType
{
  BooleanNegate,
  RealNegate,
}

/// <summary>
/// ASTNode for a unary operation.
/// </summary>
/// <param name="opSource"></param>
/// <param name="type"></param>
/// <param name="rightHandExpression"></param>
public class UnaryOp(SourceSpan opSource, UnaryOpType type, RightHandExpression rightHandExpression)
  : RightHandExpression
{
  /// <summary>
  /// The SourceSpan for the operator.
  /// </summary>
  public SourceSpan OpSource => opSource;

  /// <summary>
  /// The type of the operator.
  /// </summary>
  public UnaryOpType Type => type;

  /// <summary>
  /// The value expression to apply the operation on.
  /// </summary>
  public RightHandExpression RightHandExpression => rightHandExpression;

  /// <summary>
  /// Used to dereference and perform type checking on CSharp operators.
  /// </summary>
  private static Dictionary<UnaryOpType, string> CSMethodNames = new Dictionary<UnaryOpType, string>
  {
    { UnaryOpType.BooleanNegate, "op_LogicalNot" },
    { UnaryOpType.RealNegate, "op_UnaryNegation" },
  };

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Type).With(Type), nameof(RightHandExpression).With(RightHandExpression)];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    var opMethodName = CSMethodNames[Type];

    var operandType = RightHandExpression.TypeCheck(context).NotNull();
    var opMethod = operandType.MakeStatic().DerefFieldType(opMethodName);
    if (opMethod is DevConType devConType)
    {
      return devConType.DerefReturnType([operandType]);
    }
    return operandType;
  }

  public override object? Evaluate(ExecutionContext context)
  {
    var operand = RightHandExpression.Evaluate(context);
    if (operand?.GetType().GetMethod(CSMethodNames[Type]) is MethodInfo methodInfo)
    {
      return methodInfo.Invoke(operand, [])!;
    }
    else
    {
      dynamic dyn = operand!;
      switch (Type)
      {
        case UnaryOpType.BooleanNegate:
          return !dyn;
        case UnaryOpType.RealNegate:
          return -dyn;
        default:
          throw new NotImplementedException();
      }
    }
  }

  protected override Span _GetSpan()
  {
    return Span.Join(OpSource.GetSpan(), RightHandExpression.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [OpSource, RightHandExpression];
  }
}
