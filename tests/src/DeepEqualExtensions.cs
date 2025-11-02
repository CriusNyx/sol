using DeepEqual.Syntax;

public static class DeepEqualExtensions
{
  public static CompareSyntax<T, E> IgnoreProperty<T, E>(
    this CompareSyntax<T, E> syntax,
    Type type,
    string propName
  )
  {
    return syntax.IgnoreProperty(
      (prop) => prop.DeclaringType.IsAssignableTo(type) && prop.Name == propName
    );
  }
}
