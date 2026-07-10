using CriusNyx.Util;
using DevCon.DataStructures;
using DevCon.Execution;
using DevCon.TypeSystem;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon.AST;

/// <summary>
/// ASTNode for a LeftHandExpression.
/// </summary>
/// <param name="identifier"></param>
/// <param name="chain"></param>
public class LeftHandExpression(Identifier? identifier, LeftHandExpressionChain? chain)
  : RightHandExpression
{
  /// <summary>
  /// The Identifier that starts the LeftHandExpression.
  /// </summary>
  public Identifier? Identifier => identifier;

  /// <summary>
  /// The expression chain for the LeftHandExpression.
  /// </summary>
  public LeftHandExpressionChain? Chain => chain;

  /// <summary>
  /// Evaluate the expression as a reference so that it can be derefenced or assigned.
  /// </summary>
  /// <param name="context"></param>
  /// <returns></returns>
  public ObjectReference EvaluateReference(ExecutionContext context)
  {
    var self = new ObjectReference(context, Identifier.NotNull().Source);
    if (Chain != null) { }
    return self;
  }

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Identifier).With(Identifier)!, nameof(Chain).With(Chain)!];
  }

  protected override DevConType? _TypeCheck(TypeContext context)
  {
    var identifierType =
      Identifier?.Transform(ident => context.typeScope.GetType(Identifier.Source))
      ?? new UnknownType(null);
    Identifier?.SetType(identifierType);
    context.PushType(identifierType);
    var output = Chain == null ? identifierType : Chain.TypeCheck(context);
    context.PopType();
    return output;
  }

  /// <summary>
  /// Get the name of the left hand expression for type checking.
  /// </summary>
  /// <returns></returns>
  public string? GetLocalName()
  {
    if (Chain == null)
    {
      return Identifier?.Source;
    }
    return null;
  }

  public override object Evaluate(ExecutionContext context)
  {
    var underlying = context.GetValue(Identifier.NotNull().Source);
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

  protected override Span _GetSpan()
  {
    return Span.SafeJoin(Identifier?.GetSpan(), Chain?.GetSpan());
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return new ASTNode?[] { Identifier, Chain }.WhereAs<ASTNode>();
  }
}
