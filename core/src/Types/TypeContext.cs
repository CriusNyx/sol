using CriusNyx.Util;

namespace DevCon.TypeSystem;

public class TypeContext
{
  public TypeScope typeScope { get; private set; } = TypeScope.CreateGlobalScope();
  public Stack<DevConType> resolutionStack { get; private set; } = new Stack<DevConType>();

  public void PushScope()
  {
    typeScope = typeScope.PushScope();
  }

  public void PopScope()
  {
    typeScope = typeScope.PopScope().NotNull("scope");
  }

  public void PushType(DevConType devConType) => resolutionStack.Push(devConType);

  public DevConType PeekType() => resolutionStack.Peek();

  public DevConType PopType() => resolutionStack.Pop();
}
