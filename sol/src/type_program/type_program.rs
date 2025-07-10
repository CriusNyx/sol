use chumsky::{Parser, error::Rich};
use derive_getters::Getters;
use derive_more::From;
use logos::Logos;

use crate::{
  lsp::semantic_types::SemanticToken,
  type_program::{
    nodes::ast_node::{ASTNode, ASTNodeData},
    parser::type_program_parser,
    type_token::TypeToken,
    types::scope::Scope,
  },
};

#[derive(Debug, From, Clone)]
pub enum TypeProgramError {
  FailedToLex,
  ParseError(Vec<TypeToken>, Vec<Rich<'static, TypeToken>>),
}

#[derive(Debug, Getters)]
pub struct TypeProgram {
  ast: ASTNode,
}

impl TypeProgram {
  pub fn lex(source: &str) -> Result<Vec<TypeToken>, TypeProgramError> {
    TypeToken::lexer(source)
      .collect::<Result<Vec<_>, ()>>()
      .map_err(|_| TypeProgramError::FailedToLex)
  }

  pub fn parse(
    lex_result: Result<Vec<TypeToken>, TypeProgramError>,
  ) -> Result<TypeProgram, TypeProgramError> {
    lex_result.and_then(|x| {
      type_program_parser()
        .parse(x.as_ref())
        .into_result()
        .map(|ast| TypeProgram { ast })
        .map_err(|e| {
          TypeProgramError::ParseError(
            x.clone(),
            e.iter().map(|y| y.clone().into_owned()).collect::<Vec<_>>(),
          )
        })
    })
  }

  pub fn compile(source: &str) -> Result<TypeProgram, TypeProgramError> {
    Self::parse(Self::lex(source)).map(|x| {
      x.compute_types();
      x
    })
  }

  fn compute_types(&self) {
    self.ast.calc_type(None);
  }

  pub fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    ASTNode::update_semantics(&self.ast, tokens);
  }

  pub fn global_scope<'a>(&'a self) -> Scope {
    todo!()
  }
}
