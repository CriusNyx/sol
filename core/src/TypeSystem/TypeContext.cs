using CriusNyx.Util;

namespace DevCon.TypeSystem;

/// <summary>
/// Context used for type checking.
/// </summary>
public class TypeContext
{
  /// <summary>
  /// Current type scope.
  /// </summary>
  public TypeScope typeScope { get; private set; } = TypeScope.CreateGlobalScope();

  /// <summary>
  /// Stack of types in the current resolution.
  /// </summary>
  public Stack<DevConType> resolutionStack { get; private set; } = new Stack<DevConType>();

  /// <summary>
  /// Push onto the scope stack.
  /// </summary>
  public void PushScope()
  {
    typeScope = typeScope.PushScope();
  }

  /// <summary>
  /// Pop off of the scope stack.
  /// </summary>
  public void PopScope()
  {
    typeScope = typeScope.PopScope().NotNull("scope");
  }

  /// <summary>
  /// Push a new type in the type resolution stack.
  /// </summary>
  /// <param name="devConType"></param>
  public void PushType(DevConType devConType) => resolutionStack.Push(devConType);

  /// <summary>
  /// Peek at the top of the type resolution stack.
  /// </summary>
  /// <returns></returns>
  public DevConType PeekType() => resolutionStack.Peek();

  /// <summary>
  /// Pop off the type resolution stack.
  /// </summary>
  /// <returns></returns>
  public DevConType PopType() => resolutionStack.Pop();
}
