using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.Execution;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// The ASTNode for an invocation expression.
/// </summary>
/// <param name="leftParen"></param>
/// <param name="arguments"></param>
/// <param name="rightParen"></param>
/// <param name="chain"></param>
public class InvocationExpression(
  SourceSpan? leftParen,
  RightHandExpression?[] arguments,
  SourceSpan? rightParen,
  LeftHandExpressionChain? chain
) : LeftHandExpressionChain
{
  /// <summary>
  /// The SourceSpan for the left paren.
  /// </summary>
  public SourceSpan? LeftParen => leftParen;

  /// <summary>
  /// The set of argument expressions used to invoke the method.
  /// </summary>
  public IEnumerable<RightHandExpression?> Arguments => arguments;

  /// <summary>
  /// The SourceSpan for the right paren.
  /// </summary>
  public SourceSpan? RightParen => rightParen;

  /// <summary>
  /// The Chain expression for the next part of the LeftHandExpression.
  /// </summary>
  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Arguments).With(Arguments), nameof(Chain).With(Chain)!];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    List<DevConType> args = new List<DevConType>();
    var underlyingType = context.PeekType();
    foreach (var arg in Arguments)
    {
      context.PushScope();
      var result = arg?.TypeCheck(context).NotNull() ?? new UnknownType(underlyingType);
      args.Add(result);
      context.PopScope();
    }

    context.PushType(underlyingType);
    var output = underlyingType.DerefReturnType(args.ToArray());
    context.PopType();
    return output;
  }

  public override object Evaluate(object underlying, ExecutionContext context)
  {
    if (underlying is FunctionValue func)
    {
      return func.Invoke(Arguments.Select(x => x.NotNull().Evaluate(context)).ToArray()!)!;
    }
    throw new NotImplementedException();
  }

  protected override Span _GetSpan()
  {
    return Span.SafeJoin(
      LeftParen?.GetSpan(),
      Span.SafeJoin(Arguments.Select(x => x?.GetSpan()).ToArray()),
      RightParen?.GetSpan(),
      Chain?.GetSpan()
    );
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { LeftParen }
      .Concat(Arguments)
      .Concat([RightParen, Chain])
      .WhereAs<ASTNode>();
  }
}
