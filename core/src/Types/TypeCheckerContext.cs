using CriusNyx.Util;

namespace Sol.TypeSystem;

public class TypeCheckerContext
{
  public TypeScope typeScope { get; private set; } = TypeScope.CreateGlobalScope();
  public Stack<SolType> resolutionStack { get; private set; } = new Stack<SolType>();

  public void PushScope()
  {
    typeScope = typeScope.PushScope();
  }

  public void PopScope()
  {
    typeScope = typeScope.PopScope().NotNull("scope");
  }

  public void PushType(SolType solType) => resolutionStack.Push(solType);

  public SolType PeekType() => resolutionStack.Peek();

  public SolType PopType() => resolutionStack.Pop();
}
