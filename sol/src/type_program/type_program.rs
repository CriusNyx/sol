use chumsky::{Parser, error::Rich};
use derive_getters::Getters;
use derive_more::From;
use logos::Logos;

use crate::{
  lsp::semantic_types::SemanticToken,
  type_program::{nodes::ast_node::ASTNode, parser::type_program_parser},
  type_program_old::TypeToken,
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

  pub fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    ASTNode::update_semantics(&self.ast, tokens);
  }
}
