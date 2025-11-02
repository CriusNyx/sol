using CriusNyx.Util;
using Sol.AST;

public enum SemanticType
{
  None,
  Keyword,
  ClassReference,
  ObjectReference,
  MethodReference,
  StringLit,
  NumLit,
}

public class SemanticToken(Span span, SemanticType type)
{
  public Span Span => span;
  public SemanticType Type => type;
}

public static class SemanticsAnalysis
{
  public static IEnumerable<(string source, SemanticToken token)> Stream(
    this IEnumerable<SemanticToken> list,
    string source
  )
  {
    int current = 0;
    foreach (var element in list)
    {
      if (element.Span.start > current)
      {
        var delta = element.Span - current;
        yield return source.Substring(delta).With(new SemanticToken(delta, SemanticType.None));

        current = element.Span.start;
      }
      {
        yield return source.Substring(element.Span).With(element);
        current = element.Span.End;
      }
    }

    if (current != source.Length)
    {
      var delta = new Span(current, source.Length - current);
      yield return source.Substring(delta).With(new SemanticToken(delta, SemanticType.None));
    }
  }

  public static IEnumerable<SemanticToken> SemanticsSafe(this ASTNode? node)
  {
    if (node != null)
    {
      return node.Semantics();
    }
    return [];
  }

  private static IEnumerable<SemanticToken> GetSemantics_LeftHandExpression(LeftHandExpression lhe)
  {
    var ident = lhe.Identifier;
    return new SemanticToken(ident.GetSpan(), ident.NodeType.ToSemanticType()).ThenConcat(
      lhe.Chain.SemanticsSafe()
    );
  }

  public static IEnumerable<SemanticToken> Semantics(this ASTNode node)
  {
    if (node is SolProgram program)
    {
      return GetSemantics_Program(program);
    }
    else if (node is UseStatement useStatement)
    {
      return GetSemantics_Use(useStatement);
    }
    else if (node is LeftHandExpression lhe)
    {
      return GetSemantics_LeftHandExpression(lhe);
    }
    else if (node is LeftHandExpressionChain chain)
    {
      return GetSemantics_LeftHandExpressionChain(chain);
    }
    else if (node is StringLiteralExpression stringLit)
    {
      return GetSemantics_StringLit(stringLit);
    }
    else if (node is NumberLiteralExpression numLit)
    {
      return GetSemantics_NumLit(numLit);
    }
    throw new NotImplementedException();
  }

  private static IEnumerable<SemanticToken> GetSemantics_Program(SolProgram program)
  {
    return program.Statements.SelectMany(x => x.Semantics());
  }

  private static IEnumerable<SemanticToken> GetSemantics_Use(UseStatement useStatement)
  {
    return new SemanticToken(useStatement.UseKeyword.GetSpan(), SemanticType.Keyword).ThenConcat(
      useStatement.NamespaceSequence.Select(ns => new SemanticToken(
        ns.GetSpan(),
        SemanticType.ClassReference
      ))
    );
  }

  private static IEnumerable<SemanticToken> GetSemantics_LeftHandExpressionChain(
    LeftHandExpressionChain lhe
  )
  {
    if (lhe is DerefExpression { Identifier: var ident } deref)
    {
      return new SemanticToken(ident.GetSpan(), ident.NodeType.ToSemanticType()).ThenConcat(
        deref.Chain.SemanticsSafe()
      );
    }
    else if (lhe is InvocationExpression invocation)
    {
      return invocation.Arguments.SelectMany(arg => arg.Semantics());
    }
    else
    {
      throw new NotImplementedException();
    }
  }

  private static IEnumerable<SemanticToken> GetSemantics_StringLit(
    StringLiteralExpression stringLit
  )
  {
    return new SemanticToken(stringLit.GetSpan(), SemanticType.StringLit).AsArray();
  }

  private static IEnumerable<SemanticToken> GetSemantics_NumLit(NumberLiteralExpression numLit)
  {
    return new SemanticToken(numLit.GetSpan(), SemanticType.NumLit).AsArray();
  }
}
