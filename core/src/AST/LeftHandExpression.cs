using CriusNyx.Util;
using Sol.DataStructures;
using Sol.Execution;
using Sol.TypeSystem;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol.AST;

public class LeftHandExpression(Identifier identifier, LeftHandExpressionChain? chain)
  : RightHandExpression
{
  public Identifier Identifier => identifier;
  public LeftHandExpressionChain? Chain => chain;

  public ObjectReference EvaluateReference(ExecutionContext context)
  {
    var self = new ObjectReference(context, Identifier.Source);
    if (Chain != null) { }
    return self;
  }

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Identifier).With(Identifier), nameof(Chain).With(Chain)!];
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    var identifierType = context.typeScope.GetType(Identifier.Source) ?? new NullType();
    Identifier.SetType(identifierType);
    context.PushType(identifierType);
    var output = Chain == null ? identifierType : Chain.TypeCheck(context);
    context.PopType();
    return output;
  }

  public string? GetLocalName()
  {
    if (Chain == null)
    {
      return Identifier.Source;
    }
    return null;
  }

  public override object Evaluate(ExecutionContext context)
  {
    var underlying = context.GetValue(Identifier.Source);
    if (chain != null)
    {
      underlying = chain.Evaluate(underlying, context);
    }
    if (underlying is decimal d)
    {
      return new NumVal(d);
    }
    return underlying;
  }

  public override Span GetSpan()
  {
    return Span.SafeJoin(Identifier.GetSpan(), Chain?.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    yield return Identifier;
    if (Chain != null)
    {
      yield return Chain;
    }
  }
}
