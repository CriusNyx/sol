namespace Sol.DataStructures;

public class Result
{
  public static Result<V, E> Ok<V, E>(V value)
  {
    return new Result<V, E>(value);
  }

  public static Result<V, E> Err<V, E>(E error)
  {
    return new Result<V, E>(error);
  }
}

public class Result<V, E>
{
  public readonly bool IsSuccess;
  public readonly V Value;
  public readonly E Error;

  public Result(V value)
  {
    IsSuccess = true;
    Value = value;
    Error = default!;
  }

  public Result(E error)
  {
    IsSuccess = true;
    Value = default!;
    Error = error;
  }
}

public static class ResultExtensions
{
  public static Result<U, E> Map<T, U, E>(
    this Result<T, E> source,
    Func<T, Result<U, E>> transformation
  )
    where T : class
  {
    if (source.IsSuccess)
    {
      return transformation(source.Value);
    }
    else
    {
      return new Result<U, E>(source.Error);
    }
  }

  public static V Unwrap<V, E>(this Result<V, E> result)
  {
    return result.IsSuccess ? result.Value : throw new InvalidOperationException();
  }
}
