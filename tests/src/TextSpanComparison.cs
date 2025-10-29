using DeepEqual;
using Superpower.Model;

namespace Sol.Tests;

public class TextSpanComparison : IComparison
{
  public bool CanCompare(Type type1, Type type2)
  {
    return type1 == typeof(TextSpan) || type2 == typeof(TextSpan);
  }

  public (ComparisonResult result, IComparisonContext context) Compare(
    IComparisonContext context,
    object value1,
    object value2
  )
  {
    if (value1 is TextSpan t1 && value2 is TextSpan t2)
    {
      if (t1.ToString() == t2.ToString())
      {
        return (ComparisonResult.Pass, context);
      }
    }
    return (ComparisonResult.Fail, context);
  }
}
