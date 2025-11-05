using CriusNyx.Util;

namespace Sol.DataStructures;

public class Result
{
  public static OkResult<T> Ok<T>(T value)
  {
    return new OkResult<T>(value);
  }

  public static Result<T, E> Ok<T, E>(T value)
  {
    return new Result<T, E>(true, value, default!);
  }

  public static Result<T, E> Err<T, E>(E error)
  {
    return new Result<T, E>(false, default!, error);
  }
}

public class OkResult<T>(T value)
{
  public T Value => value;
}

public class ErrorResult<E>(E error)
{
  public E Error => error;
}

public class Result<T, E>
{
  public readonly bool IsSuccess;
  public readonly T Value;
  public readonly E Error;

  public Result(bool isSuccess, T value, E error)
  {
    IsSuccess = isSuccess;
    Value = value;
    Error = error;
  }

  public static implicit operator Result<T, E>(T ok)
  {
    return new Result<T, E>(true, ok, default!);
  }

  public static implicit operator Result<T, E>(E err)
  {
    return new Result<T, E>(false, default!, err);
  }
}

public static class ResultExtensions
{
  public static T Unwrap<T, E>(this Result<T, E> result)
  {
    return result.IsSuccess ? result.Value : throw new InvalidOperationException();
  }

  public static T UnwrapOr<T, E>(this Result<T, E> result, T defaultValue)
  {
    return result.IsSuccess ? result.Value : defaultValue;
  }

  public static T UnwrapOrDefault<T, E>(this Result<T, E> result)
  {
    return UnwrapOr(result!, default)!;
  }

  public static T UnwrapOrElse<T, E>(this Result<T, E> result, Func<E, T> orElse)
  {
    return result.IsSuccess ? result.Value : orElse(result.Error);
  }

  public static Result<U, E> Map<T, U, E>(this Result<T, E> source, Func<T, U> transformation)
  {
    if (source.IsSuccess)
    {
      return Result.Ok<U, E>(transformation(source.Value));
    }
    else
    {
      return Result.Err<U, E>(source.Error);
    }
  }

  public static Result<T, F> MapErr<T, E, F>(this Result<T, E> result, Func<E, F> mapErr)
  {
    if (result.IsSuccess)
    {
      return Result.Ok<T, F>(result.Value);
    }
    else
    {
      return Result.Err<T, F>(mapErr(result.Error));
    }
  }

  /// <summary>
  /// If self is successful return other. Otherwise return self as err.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <typeparam name="E"></typeparam>
  /// <param name="self"></param>
  /// <param name="then"></param>
  /// <returns></returns>
  public static Result<U, E> And<T, U, E>(this Result<T, E> self, Result<U, E> other)
  {
    return self.AndThen(_ => other);
  }

  /// <summary>
  /// If self is successful returns self as ok. Otherwise returns other.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="E"></typeparam>
  /// <typeparam name="F"></typeparam>
  /// <param name="self"></param>
  /// <param name="other"></param>
  /// <returns></returns>
  public static Result<T, F> Or<T, E, F>(this Result<T, E> self, Result<T, F> other)
  {
    return self.OrElse(_ => other);
  }

  /// <summary>
  /// If self is successful return the output of then. Otherwise return self as err.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="U"></typeparam>
  /// <typeparam name="E"></typeparam>
  /// <param name="self"></param>
  /// <param name="then"></param>
  /// <returns></returns>
  public static Result<U, E> AndThen<T, U, E>(this Result<T, E> self, Func<T, Result<U, E>> then)
  {
    /*
     * self     input   result   output
     * Err(e)   _       _        self
     * Ok(x)    x       Err(d)   result
     * Ok(x)    x       Ok(y)    result
    */
    if (self.IsSuccess)
    {
      return then(self.Value);
    }
    else
    {
      return Result.Err<U, E>(self.Error);
    }
  }

  /// <summary>
  /// If self is successful returns self as ok. Otherwise returns the result of orElse
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="E"></typeparam>
  /// <typeparam name="F"></typeparam>
  /// <param name="self"></param>
  /// <param name="orElse"></param>
  /// <returns></returns>
  public static Result<T, F> OrElse<T, E, F>(this Result<T, E> self, Func<E, Result<T, F>> orElse)
  {
    if (self.IsSuccess)
    {
      return Result.Ok<T, F>(self.Value);
    }
    else
    {
      return orElse(self.Error);
    }
  }

  public static Result<(T, U), E> AndWith<T, U, E>(this Result<T, E> self, Result<U, E> other)
  {
    return self.AndThen((v1) => other.Map((v2) => v1.With(v2)));
  }

  public static Result<(T, U, V), E> AndThenWith<T, U, V, E>(
    this Result<(T, U), E> self,
    Result<V, E> other
  )
  {
    return self.AndThen((v1) => other.Map((v2) => v1.AndWith(v2)));
  }

  public static Result<(T, U, V, W), E> AndThenWith<T, U, V, W, E>(
    this Result<(T, U, V), E> self,
    Result<W, E> other
  )
  {
    return self.AndThen((v1) => other.Map((v2) => v1.AndWith(v2)));
  }
}
