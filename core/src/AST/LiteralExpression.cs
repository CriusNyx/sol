using System.Numerics;
using CriusNyx.Util;
using Sol.AST;
using Superpower.Model;

public class StringLiteralExpression(SourceSpan source, string value) : RightHandExpression
{
  public SourceSpan Source => source;
  public string Value => value;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Value).With(Value)];
  }

  public override object Evaluate(ExecutionContext context)
  {
    return Value;
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    return new CSType(typeof(string));
  }

  public override Span GetSpan()
  {
    return Source.GetSpan();
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [Source];
  }

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(GetSpan(), SemanticType.StringLit)];
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

public class NumberLiteralExpression(SourceSpan source, NumVal value) : RightHandExpression
{
  public SourceSpan Source => source;
  public NumVal Value => value;

  public override IEnumerable<(string, object)> EnumerateFields()
  {
    return [nameof(Value).With(Value)];
  }

  public override object Evaluate(ExecutionContext context)
  {
    return Value;
  }

  protected override SolType? _TypeCheck(TypeCheckerContext context)
  {
    return new CSType(typeof(NumVal));
  }

  public override Span GetSpan()
  {
    return Source.GetSpan();
  }

  public override IEnumerable<ASTNode> GetChildren()
  {
    return [Source];
  }

  public override IEnumerable<SemanticToken> GetSemantics()
  {
    return [new(GetSpan(), SemanticType.NumLit)];
  }
}
