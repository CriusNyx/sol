use std::{cell::RefCell, collections::HashMap, rc::Rc};

use chumsky::{Parser, error::Rich};
use derive_getters::Getters;
use derive_more::From;
use logos::Logos;

use crate::{
  lsp::semantic_types::SemanticToken,
  type_program::{
    nodes::st_ast::{ASTNodeData, StAst},
    st_parser::type_program_parser,
    st_token::StToken,
    types::{Type, type_resolution::InstancedType},
  },
};

#[derive(Debug, From, Clone)]
pub enum TypeProgramError {
  FailedToLex,
  ParseError(Vec<StToken>, Vec<Rich<'static, StToken>>),
}

#[derive(Debug, Getters)]
pub struct TypeProgram {
  ast: StAst,
  program_type: RefCell<Option<Rc<Type>>>,
}

impl TypeProgram {
  pub fn lex(source: &str) -> Result<Vec<StToken>, TypeProgramError> {
    StToken::lexer(source)
      .collect::<Result<Vec<_>, ()>>()
      .map_err(|_| TypeProgramError::FailedToLex)
  }

  pub fn parse(
    lex_result: Result<Vec<StToken>, TypeProgramError>,
  ) -> Result<TypeProgram, TypeProgramError> {
    lex_result.and_then(|x| {
      type_program_parser()
        .parse(x.as_ref())
        .into_result()
        .map(|ast| TypeProgram {
          ast,
          program_type: RefCell::new(None),
        })
        .map_err(|e| {
          TypeProgramError::ParseError(
            x.clone(),
            e.iter().map(|y| y.clone().into_owned()).collect::<Vec<_>>(),
          )
        })
    })
  }

  pub fn parse_string(source: &str) -> Result<TypeProgram, TypeProgramError> {
    Self::parse(Self::lex(source))
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
    StAst::update_semantics(&self.ast, tokens);
  }

  pub fn get_program_type(&self) -> Rc<Type> {
    let mut program_type = self.program_type().borrow_mut();
    if let None = *program_type {
      *program_type = Some(Rc::new(self.ast.calc_type(None).1));
    }
    drop(program_type);
    self.program_type().borrow().as_ref().unwrap().clone()
  }

  pub fn get_global_types(&self) -> Rc<HashMap<String, Rc<Type>>> {
    self
      .get_program_type()
      .try_as_program_type_ref()
      .unwrap()
      .types()
      .clone()
  }

  pub fn get_global_instance(&self) -> InstancedType {
    InstancedType::new(self.get_program_type().clone(), vec![])
  }
}
