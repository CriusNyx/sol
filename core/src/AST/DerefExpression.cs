using System.Reflection;
using CriusNyx.Util;
using Sol.DataStructures;
using Sol.Execution;
using Sol.Runtime;
using Sol.TypeSystem;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol.AST;

public class DerefExpression(
  SourceSpan? dot,
  Identifier? identifier,
  LeftHandExpressionChain? chain
) : LeftHandExpressionChain
{
  public SourceSpan? Dot => dot;
  public Identifier? Identifier => identifier;
  public LeftHandExpressionChain? Chain => chain;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Identifier).With(Identifier)!, nameof(Chain).With(Chain)!];
  }

  public override ObjectReference EvaluateReference(
    ObjectReference underlying,
    ExecutionContext context
  )
  {
    var value = underlying.Get();
    var self = new ObjectReference(value!, Identifier.NotNull().Source);
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
      return Next(derefable.Deref(Identifier.NotNull().Source));
    }
    else if (underlying.GetType().GetField(Identifier.NotNull().Source) is FieldInfo field)
    {
      return Next(field.GetValue(underlying)!);
    }
    else if (underlying.GetType().GetProperty(Identifier.NotNull().Source) is PropertyInfo property)
    {
      return Next(property.GetValue(underlying)!);
    }
    else if (
      underlying.GetType().GetMember(Identifier.NotNull().Source) is MemberInfo[] members
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

  protected override SolType? _TypeCheck(TypeContext context)
  {
    var underlyingType = context.PeekType();
    var fieldType =
      Identifier?.Transform(ident =>
        underlyingType.DerefFieldType(ident.Source).NotNull("DereferencedType")
      ) ?? new UnknownType();
    Identifier?.SetType(fieldType);
    context.PushType(fieldType);
    var output = Chain == null ? fieldType : Chain.TypeCheck(context);
    context.PopType();
    return output;
  }

  public override Span GetSpan()
  {
    return Span.SafeJoin(Identifier?.GetSpan(), Chain?.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { Dot, Identifier, Chain }.WhereAs<ASTNode>();
  }
}
