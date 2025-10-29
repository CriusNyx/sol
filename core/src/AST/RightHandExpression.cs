using System.Reflection;
using CriusNyx.Util;

namespace Sol.AST;

public abstract class RightHandExpression : ASTNode { }

public class ParenExpression(RightHandExpression rightHandExpression) : RightHandExpression
{
  public RightHandExpression RightHandExpression => rightHandExpression;

  public override object? Evaluate(ExecutionContext context)
  {
    return RightHandExpression.Evaluate(context);
  }

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(RightHandExpression).With(rightHandExpression)];
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    return RightHandExpression.TypeCheck(context);
  }
}

public enum UnaryOpType
{
  BooleanNegate,
  RealNegate,
}

public class UnaryOp(UnaryOpType type, RightHandExpression rightHandExpression)
  : RightHandExpression
{
  public UnaryOpType Type => type;
  public RightHandExpression RightHandExpression => rightHandExpression;

  private static Dictionary<UnaryOpType, string> CSMethodNames = new Dictionary<UnaryOpType, string>
  {
    { UnaryOpType.BooleanNegate, "op_LogicalNot" },
    { UnaryOpType.RealNegate, "op_UnaryNegation" },
  };

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Type).With(Type), nameof(RightHandExpression).With(RightHandExpression)];
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    var opMethodName = CSMethodNames[Type];

    var operandType = RightHandExpression.TypeCheck(context).NotNull();
    var opMethod = operandType.MakeStatic().DerefFieldType(opMethodName);
    if (opMethod is SolType solType)
    {
      return solType.DerefReturnType([operandType]);
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
}

public enum BinaryOpType
{
  Add,
  Subtract,
  Multiply,
  Divide,
  Modulo,
}

public class BinaryOp(BinaryOpType type, RightHandExpression left, RightHandExpression right)
  : RightHandExpression
{
  public BinaryOpType Type => type;
  public RightHandExpression Left => left;
  public RightHandExpression Right => right;

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

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    var opMethodName = CSMethodNames[Type];

    var leftType = Left.TypeCheck(context).NotNull();
    var rightType = Right.TypeCheck(context).NotNull();
    var opMethod = leftType.MakeStatic().DerefFieldType(opMethodName);
    if (opMethod is SolType solType)
    {
      return solType.DerefReturnType([leftType, rightType]);
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
}
