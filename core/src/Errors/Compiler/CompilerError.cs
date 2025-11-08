using Sol.AST;
using Sol.Parser;

namespace Sol;

public class CompilerError
{
  public virtual ASTNode RecoverAST()
  {
    throw new NotImplementedException();
  }

  public static CompilerError From(ASTNode partialAST, ParseContext context)
  {
    return new FailedToParseError(partialAST, context.Errors.ToArray());
  }
}

public class FailedToParseError(ASTNode partialAST, ParseError[] parseErrors) : CompilerError
{
  public ASTNode PartialAST => partialAST;
  public IEnumerable<ParseError> ParseErrors => parseErrors;

  public override ASTNode RecoverAST()
  {
    return PartialAST;
  }
}
