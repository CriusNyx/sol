using CriusNyx.Util;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

  public T Unwrap()
  {
    return IsSuccess ? Value : throw new InvalidOperationException();
  }

  public T UnwrapOr(T defaultValue)
  {
    return IsSuccess ? Value : defaultValue;
  }

  public T UnwrapOrDefault()
  {
    return UnwrapOr(default!);
  }

  public T UnwrapOrElse(Func<E, T> orElse)
  {
    return IsSuccess ? Value : orElse(Error);
  }

  public Result<U, E> Map<U>(Func<T, U> transformation)
  {
    if (IsSuccess)
    {
      return Result.Ok<U, E>(transformation(Value));
    }
    else
    {
      return Result.Err<U, E>(Error);
    }
  }

  public Result<T, F> MapErr<F>(Func<E, F> mapErr)
  {
    if (IsSuccess)
    {
      return Result.Ok<T, F>(Value);
    }
    else
    {
      return Result.Err<T, F>(mapErr(Error));
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
  public Result<U, E> And<U>(Result<U, E> other)
  {
    return AndThen(_ => other);
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
  public Result<T, F> Or<F>(Result<T, F> other)
  {
    return OrElse(_ => other);
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
  public Result<U, E> AndThen<U>(Func<T, Result<U, E>> then)
  {
    /*
     * self     input   result   output
     * Err(e)   _       _        self
     * Ok(x)    x       Err(d)   result
     * Ok(x)    x       Ok(y)    result
    */
    if (IsSuccess)
    {
      return then(Value);
    }
    else
    {
      return Result.Err<U, E>(Error);
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
  public Result<T, F> OrElse<F>(Func<E, Result<T, F>> orElse)
  {
    if (IsSuccess)
    {
      return Result.Ok<T, F>(Value);
    }
    else
    {
      return orElse(Error);
    }
  }
}

public static class ResultExtensions
{
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
