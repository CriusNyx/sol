namespace Sol.Parser;

public class ParseResult<T>
{
  public T value;
  public ParseContext context;

  public ParseResult(T value)
  {
    this.value = value;
    this.context = new ParseContext();
  }

  public static ParseResult<T> Ok(T value)
  {
    return new ParseResult<T>(value);
  }
}
