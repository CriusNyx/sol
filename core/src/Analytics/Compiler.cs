using DevCon.AST;
using DevCon.DataStructures;
using DevCon.Parser;
using DevCon.TypeSystem;
using static DevCon.DataStructures.Result;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon;

/// <summary>
/// Error for when the source could not be compiled but could be compiled partially.
/// </summary>
public class PartialCompileError(ASTNode astNode) : CompilerError
{
  public ASTNode AST => astNode;
}

public class TypeCheckResult(string source, ASTNode astNode, TypeContext context)
{
  public string Source => source;
  public ASTNode AST => astNode;
  public TypeContext Context => context;
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
    TypeContext context = null!
  )
  {
    return TypeCheck(Parse(source), context);
  }

  public static Result<TypeCheckResult, CompilerError> TypeCheck(
    Result<ParseResult, CompilerError> compilation,
    TypeContext context = null!
  )
  {
    return compilation.AndThen(
      (compile) =>
      {
        try
        {
          context = context ?? new TypeContext();
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
    return DevConParser.Parse(source).Map(ast => new ParseResult(source, ast));
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
