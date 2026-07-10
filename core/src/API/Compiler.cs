using CriusNyx.Results;
using DevCon.AST;
using DevCon.Parser;
using DevCon.TypeSystem;
using static CriusNyx.Results.Result;
using ExecutionContext = DevCon.Execution.ExecutionContext;

namespace DevCon;

/// <summary>
/// Successful result for type checker.
/// </summary>
/// <param name="source"></param>
/// <param name="astNode"></param>
/// <param name="context"></param>
public class TypeCheckResult(string source, ASTNode astNode, TypeContext context)
{
  /// <summary>
  /// The source code of the program that was type checked.
  /// </summary>
  public string Source => source;

  /// <summary>
  /// The AST of the program that was type checked.
  /// </summary>
  public ASTNode AST => astNode;

  /// <summary>
  /// The context with the type information for the program.
  /// </summary>
  public TypeContext Context => context;
}

/// <summary>
/// Successful result for parsing.
/// </summary>
/// <param name="source"></param>
/// <param name="astNode"></param>
public class ParseResult(string source, ASTNode astNode)
{
  /// <summary>
  /// The source of the program that was parsed.
  /// </summary>
  public string Source => source;

  /// <summary>
  /// The AST of the program that was parsed.
  /// </summary>
  public ASTNode AST => astNode;
}

/// <summary>
/// Successful result for executing a program.
/// </summary>
/// <param name="source"></param>
/// <param name="astNode"></param>
/// <param name="executionContext"></param>
/// <param name="result"></param>
public class EvaluationResult(
  string source,
  ASTNode astNode,
  ExecutionContext executionContext,
  object? result
)
{
  /// <summary>
  /// The source of the program that was evaluated.
  /// </summary>
  public string source = source;

  /// <summary>
  /// The AST of the program that was evaluated.
  /// </summary>
  public ASTNode AST => astNode;

  /// <summary>
  /// The execution context used to evaluate the program.
  /// This can be saved to evaluate a new program inharriting the previous scope and context.
  /// </summary>
  public ExecutionContext ExecutionContext => executionContext;

  /// <summary>
  /// The result of the last statement evaluated by the program.
  /// </summary>
  public object? Result => result;
}

/// <summary>
/// DevCon Compiler.
/// </summary>
public static class Compiler
{
  /// <summary>
  /// Evaluate the program and return the value of the last statement evaluated, or a CompilerError.
  /// </summary>
  /// <param name="source"></param>
  /// <returns></returns>
  public static Result<EvaluationResult, CompilerError> Evaluate(string source)
  {
    return TypeCheck(source)
      .Map(x =>
      {
        var context = new ExecutionContext();
        var output = x.AST.Evaluate(context);
        return new EvaluationResult(source, x.AST, context, output);
      });
  }

  /// <summary>
  /// Type check the program and return a result with the type checked program, or a CompilerError.
  /// </summary>
  /// <param name="source"></param>
  /// <param name="context"></param>
  /// <returns></returns>
  public static Result<TypeCheckResult, CompilerError> TypeCheck(
    string source,
    TypeContext context = null!
  )
  {
    return TypeCheck(Parse(source), context);
  }

  /// <summary>
  /// Type check the program and return a result with the type checked program, or a CompilerError.
  /// </summary>
  /// <param name="compilation"></param>
  /// <param name="context"></param>
  /// <returns></returns>
  public static Result<TypeCheckResult, CompilerError> TypeCheck(
    Result<ParseResult, CompilerError> compilation,
    TypeContext context = null!
  )
  {
    TypeContext TypeCheck(ASTNode ast)
    {
      context = context ?? new TypeContext();
      ast.TypeCheck(context);

      return context;
    }

    return compilation
      .AndThen(
        (compile) =>
        {
          try
          {
            context = TypeCheck(compile.AST);
            return Ok<TypeCheckResult, CompilerError>(new(compile.Source, compile.AST, context));
          }
          catch
          {
            return Err<TypeCheckResult, CompilerError>(new CompilerError(compile.AST));
          }
        }
      )
      .MapErr(
        (e) =>
        {
          var ast = e.ast;
          context = TypeCheck(ast);
          return new CompilerError(ast);
        }
      );
  }

  /// <summary>
  /// Parse the program and return the parsed program, or a CompilerError.
  /// </summary>
  /// <param name="source"></param>
  /// <returns></returns>
  public static Result<ParseResult, CompilerError> Parse(string source)
  {
    return DevConParser.Parse(source).Map(ast => new ParseResult(source, ast));
  }
}
