mod ast_visitor;
mod class_decl;
mod generic_param_decl;
mod lexon_set;
mod method_decl;
mod type_lexer;
mod type_program;
mod type_program_print;
mod type_ref;

pub use ast_visitor::*;
use chumsky::prelude::*;
pub use class_decl::*;
pub use generic_param_decl::*;
pub use lexon_set::*;
use logos::Logos;
pub use method_decl::*;
pub use type_lexer::*;
pub use type_program::*;
pub use type_program_print::*;
pub use type_ref::*;

#[derive(Debug)]
pub struct ParseError<'a> {
  pub parse_errors: Vec<Rich<'a, TypeToken<'a>>>,
  pub tokens: &'a Vec<TypeToken<'a>>,
}

#[derive(Debug)]
pub enum CompileError<'a> {
  LexError,
  ParseError(ParseError<'a>),
}

pub fn lex_type_program<'a>(source: &'a str) -> Result<Vec<TypeToken<'a>>, CompileError<'a>> {
  let result = TypeToken::lexer(source).collect::<Result<Vec<TypeToken<'a>>, _>>();
  match result {
    Ok(mut vec) => {
      for (i, e) in vec.iter_mut().enumerate() {
        e.get_info_mut().index = i as i32;
      }
      Ok(vec)
    }
    Err(_) => Err(CompileError::LexError),
  }
}

pub fn parse_type_program<'a>(
  tokens: &'a Result<Vec<TypeToken<'a>>, CompileError<'a>>,
) -> Result<TypeProgram<'a>, CompileError<'a>> {
  match tokens {
    Ok(vec) => {
      let parsed = type_parser().parse(&vec);
      match parsed.into_result() {
        Ok(program) => Ok(program),
        Err(errs) => Err(CompileError::ParseError(ParseError {
          parse_errors: errs,
          tokens: vec,
        })),
      }
    }
    Err(CompileError::LexError) => Err(CompileError::LexError),
    _ => panic!(),
  }
}
