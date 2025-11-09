using DeepEqual;
using DevCon.DataStructures;
using Superpower.Model;

namespace DevCon.Tests;

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
      return (ComparisonResult.Pass, context);
    }
    return (ComparisonResult.Fail, context);
  }
}

public class SpanComparison : IComparison
{
  public bool CanCompare(Type type1, Type type2)
  {
    return type1 == typeof(Span) || type2 == typeof(Span);
  }

  public (ComparisonResult result, IComparisonContext context) Compare(
    IComparisonContext context,
    object value1,
    object value2
  )
  {
    if (value1 is Span && value2 is Span)
    {
      return (ComparisonResult.Pass, context);
    }
    return (ComparisonResult.Fail, context);
  }
}
