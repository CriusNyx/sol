using System.Reflection;
using CriusNyx.Util;

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

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    var identifierType = context.typeScope.GetType(Identifier.Span.ToString()) ?? new NullType();
    context.PushType(identifierType);
    var output = Chain == null ? identifierType : Chain.TypeCheck(context);
    context.PopType();
    return output;
  }

  public string? GetLocalName()
  {
    if (Chain == null)
    {
      return Identifier.Span.ToString();
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
}

public abstract class LeftHandExpressionChain() : ASTNode
{
  public override object? Evaluate(ExecutionContext context)
  {
    throw new NotImplementedException();
  }

  public virtual ObjectReference EvaluateReference(
    ObjectReference underlying,
    ExecutionContext context
  )
  {
    throw new NotImplementedException();
  }

  public abstract object Evaluate(object underlying, ExecutionContext context);
}

public class DerefExpression(Identifier identifier, LeftHandExpressionChain? chain)
  : LeftHandExpressionChain
{
  public Identifier Identifier => identifier;
  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Identifier).With(Identifier), nameof(Chain).With(Chain)!];
  }

  public override ObjectReference EvaluateReference(
    ObjectReference underlying,
    ExecutionContext context
  )
  {
    var value = underlying.Get();
    var self = new ObjectReference(value!, Identifier.Source);
    if (Chain != null)
    {
      return Chain.EvaluateReference(self, context);
    }
    return self;
  }

  public override object Evaluate(object underlying, ExecutionContext context)
  {
    object Next(object self)
    {
      if (Chain != null)
      {
        return Chain.Evaluate(self, context);
      }
      return self;
    }

    if (underlying is IDeref derefable)
    {
      return Next(derefable.Deref(Identifier.Source));
    }
    else if (underlying.GetType().GetField(Identifier.Source) is FieldInfo field)
    {
      return Next(field.GetValue(underlying)!);
    }
    else if (underlying.GetType().GetProperty(Identifier.Source) is PropertyInfo property)
    {
      return Next(property.GetValue(underlying)!);
    }
    else if (
      underlying.GetType().GetMember(Identifier.Source) is MemberInfo[] members
      && members.Length > 0
      && members.All(x => x is MethodInfo)
    )
    {
      return Next(
        new MethodGroupReference(underlying, members.Select(x => x as MethodInfo).ToArray()!)
      );
    }
    throw new NotImplementedException();
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    var underlyingType = context.PeekType();
    var result = underlyingType.DerefFieldType(Identifier.Source).NotNull("DereferencedType");
    context.PushType(result);
    var output = Chain == null ? result : Chain.TypeCheck(context);
    context.PopType();
    return output;
  }
}

public class DeindexExpression(RightHandExpression index, LeftHandExpressionChain? chain)
  : LeftHandExpressionChain
{
  public RightHandExpression Index => index;
  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Index).With(index), nameof(Chain).With(chain)!];
  }

  public override object Evaluate(object underlying, ExecutionContext context)
  {
    dynamic dyn = underlying;
    var index = Evaluate(Index, context);
    return dyn[index];
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    throw new NotImplementedException();
  }
}

public class InvocationExpression(RightHandExpression[] arguments, LeftHandExpressionChain? chain)
  : LeftHandExpressionChain
{
  public IEnumerable<RightHandExpression> Arguments => arguments;

  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Arguments).With(Arguments), nameof(Chain).With(Chain)!];
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    List<SolType> args = new List<SolType>();
    foreach (var arg in Arguments)
    {
      context.PushScope();
      var result = arg.TypeCheck(context).NotNull();
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
      return func.Invoke(Arguments.Select(x => x.Evaluate(context)).ToArray()!)!;
    }
    throw new NotImplementedException();
  }
}
