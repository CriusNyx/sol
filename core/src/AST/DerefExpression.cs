using System.Reflection;
using CriusNyx.Util;
using Sol.DataStructures;
using Sol.Execution;
using Sol.Runtime;
using Sol.TypeSystem;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol.AST;

public class DerefExpression(SourceSpan dot, Identifier identifier, LeftHandExpressionChain? chain)
  : LeftHandExpressionChain
{
  public SourceSpan Dot => dot;
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

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    var underlyingType = context.PeekType();
    var fieldType = underlyingType.DerefFieldType(Identifier.Source).NotNull("DereferencedType");
    Identifier.SetType(fieldType);
    context.PushType(fieldType);
    var output = Chain == null ? fieldType : Chain.TypeCheck(context);
    context.PopType();
    return output;
  }

  public override Span GetSpan()
  {
    return Span.SafeJoin(Identifier.GetSpan(), Chain?.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    yield return Dot;
    yield return Identifier;
    if (Chain != null)
    {
      yield return Chain;
    }
  }
}
