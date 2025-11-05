using Sol.AST;
using Sol.DataStructures;
using Sol.Execution;
using Sol.Parser;
using Sol.TypeSystem;
using static Sol.DataStructures.Result;
using ExecutionContext = Sol.Execution.ExecutionContext;

namespace Sol;

public class CompilerError { }

/// <summary>
/// Error for when the source could not be compiled but could be compiled partially.
/// </summary>
public class PartialCompileError(ASTNode astNode) : CompilerError
{
  public ASTNode AST => astNode;
}

public class TypeCheckResult(string source, ASTNode astNode, TypeCheckerContext context)
{
  public string Source => source;
  public ASTNode AST => astNode;
  public TypeCheckerContext Context => context;
}

public class ParseResult(string source, ASTNode astNode)
{
  public string Source => source;
  public ASTNode AST => astNode;
}

public class ExecutionResult(
  string source,
  ASTNode astNode,
  ExecutionContext executionContext,
  object? result
)
{
  public string source = source;
  public ASTNode AST => astNode;
  public ExecutionContext ExecutionContext => executionContext;
  public object? Result => result;
}

public static class Compiler
{
  public static Result<TypeCheckResult, CompilerError> TypeCheck(
    string source,
    TypeCheckerContext context = null!
  )
  {
    return TypeCheck(Parse(source), context);
  }

  public static Result<TypeCheckResult, CompilerError> TypeCheck(
    Result<ParseResult, CompilerError> compilation,
    TypeCheckerContext context = null!
  )
  {
    return compilation.AndThen(
      (compile) =>
      {
        try
        {
          context = context ?? new TypeCheckerContext();
          compile.AST.TypeCheck(context);
          return Ok<TypeCheckResult, CompilerError>(new(compile.Source, compile.AST, context));
        }
        catch
        {
          return Err<TypeCheckResult, CompilerError>(new PartialCompileError(compile.AST));
        }
      }
    );
  }

  public static Result<ParseResult, CompilerError> Parse(string source)
  {
    try
    {
      if (SolParser.TryParse(source, out var ast))
      {
        return Ok<ParseResult, CompilerError>(new(source, ast));
      }
      return Err<ParseResult, CompilerError>(new());
    }
    catch
    {
      return Err<ParseResult, CompilerError>(new());
    }
  }

  public static Result<ExecutionResult, CompilerError> Evaluate(string source)
  {
    return TypeCheck(source)
      .Map(x =>
      {
        var context = new ExecutionContext();
        var output = x.AST.Evaluate(context);
        return new ExecutionResult(source, x.AST, context, output);
      });
  }
}
