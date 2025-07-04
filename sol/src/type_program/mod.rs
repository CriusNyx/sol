use chumsky::prelude::*;

mod ast_visitor;
mod class_decl;
mod generic_param_decl;
mod global_val;
mod identifier_decl;
mod lambda_decl;
mod method_decl;
mod type_program;
mod type_program_print;
mod type_ref;
mod type_token;

pub use ast_visitor::*;
pub use class_decl::*;
pub use generic_param_decl::*;
pub use global_val::*;
pub use identifier_decl::*;
pub use lambda_decl::*;
use logos::Logos;
pub use method_decl::*;
pub use type_program::*;
pub use type_program_print::*;
pub use type_ref::*;
pub use type_token::*;

use crate::type_system::{InterpreterResult, ObjectType, Type, TypeProgramInterpreter};

#[derive(Debug)]
pub struct ParseError<'a> {
  pub parse_errors: Vec<Rich<'a, TypeToken>>,
  pub tokens: &'a Vec<TypeToken>,
}

#[derive(Debug)]
pub enum CompileError<'a> {
  LexError,
  ParseError(ParseError<'a>),
  InterpreterError,
}

pub fn lex_type_program<'a>(source: &'a str) -> Result<Vec<TypeToken>, CompileError<'a>> {
  let result = TypeToken::lexer(&source).collect::<Result<Vec<TypeToken>, _>>();
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
  tokens: &'a Result<Vec<TypeToken>, CompileError<'a>>,
) -> Result<TypeProgram, CompileError<'a>> {
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

pub fn evaluate_type_program<'a>(
  program: &'a Result<TypeProgram, CompileError<'a>>,
) -> Result<ObjectType, CompileError<'a>> {
  match program {
    Ok(program) => match program.evaluate() {
      InterpreterResult::Identifier(_, Type::ObjectType(result)) => Ok(result),
      _ => todo!(),
    },
    Err(_) => Err(CompileError::InterpreterError),
  }
}
