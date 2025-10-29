using System.Numerics;
using CriusNyx.Util;
using Sol.AST;

public class StringLiteralExpression(string value) : RightHandExpression
{
  public string Value => value;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Value).With(Value)];
  }

  public override object Evaluate(ExecutionContext context)
  {
    return Value;
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    return new CSType(typeof(string));
  }
}

public class NumVal(decimal value) : DebugPrint
{
  public decimal Value => value;

  public static implicit operator int(NumVal numVal)
  {
    return (int)numVal.Value;
  }

  public static implicit operator decimal(NumVal numVal)
  {
    return numVal.Value;
  }

  public static implicit operator float(NumVal numVal)
  {
    return (float)numVal.Value;
  }

  public static implicit operator double(NumVal numVal)
  {
    return (double)numVal.Value;
  }

  public static NumVal operator -(NumVal a)
  {
    return new NumVal(-a.Value);
  }

  public static NumVal operator +(NumVal a, NumVal b)
  {
    return new NumVal(a.Value + b.Value);
  }

  public static NumVal operator -(NumVal a, NumVal b)
  {
    return new NumVal(a.Value - b.Value);
  }

  public static NumVal operator *(NumVal a, NumVal b)
  {
    return new NumVal(a.Value * b.Value);
  }

  public static NumVal operator /(NumVal a, NumVal b)
  {
    return new NumVal(a.Value / b.Value);
  }

  public static NumVal operator %(NumVal a, NumVal b)
  {
    return new NumVal(a.Value % b.Value);
  }

  public IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Value).With(Value)];
  }

  public override string ToString()
  {
    return value.ToString();
  }
}

public class NumberLiteralExpression(NumVal value) : RightHandExpression
{
  public NumVal Value => value;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Value).With(Value)];
  }

  public override object Evaluate(ExecutionContext context)
  {
    return Value;
  }

  public override SolType? TypeCheck(TypeCheckerContext context)
  {
    return new CSType(typeof(NumVal));
  }
}
