using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.Execution;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

public class InvocationExpression(
  SourceSpan? leftParen,
  RightHandExpression?[] arguments,
  SourceSpan? rightParen,
  LeftHandExpressionChain? chain
) : LeftHandExpressionChain
{
  public SourceSpan? LeftParen => leftParen;

  public IEnumerable<RightHandExpression?> Arguments => arguments;
  public SourceSpan? RightParen => rightParen;

  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Arguments).With(Arguments), nameof(Chain).With(Chain)!];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    List<DevConType> args = new List<DevConType>();
    foreach (var arg in Arguments)
    {
      context.PushScope();
      var result = arg?.TypeCheck(context).NotNull() ?? new UnknownType();
      args.Add(result);
      context.PopScope();
    }
    var underlyingType = context.PeekType();
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

  public override Span GetSpan()
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
