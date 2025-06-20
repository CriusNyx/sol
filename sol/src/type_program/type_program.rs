use chumsky::Parser;
use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::*;

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct TypeProgram<'token> {
  pub expressions: Vec<ClassDecl<'token>>,
}

impl<'token> PrintSource for TypeProgram<'token> {
  fn print_source(&self) -> String {
    (*self.expressions)
      .into_iter()
      .map(|f| f.print_source())
      .collect::<Vec<_>>()
      .join("\n\n")
      .clone()
  }
}

pub fn type_parser<'a>()
-> impl Parser<'a, &'a [TypeToken<'a>], TypeProgram<'a>, extra::Err<Rich<'a, TypeToken<'a>>>> {
  let class_parser = class_decl_parser();

  class_parser
    .repeated()
    .collect::<Vec<_>>()
    .map(|f| TypeProgram { expressions: f })
}
