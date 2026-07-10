namespace DevCon.Parser;

/// <summary>
/// Parsing result
/// </summary>
/// <typeparam name="T"></typeparam>
[Obsolete]
public class ParseResult<T>
{
  /// <summary>
  /// Value
  /// </summary>
  public T value;

  /// <summary>
  /// Context
  /// </summary>
  public ParseContext context;

  /// <summary>
  /// Create a new parse result.
  /// </summary>
  /// <param name="value"></param>
  public ParseResult(T value)
  {
    this.value = value;
    this.context = new ParseContext();
  }

  /// <summary>
  /// ?
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  public static ParseResult<T> Ok(T value)
  {
    return new ParseResult<T>(value);
  }
}
