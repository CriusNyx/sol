use chumsky::Parser;
use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program_old::*;

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub enum GlobalExp {
  ClassDec(ClassDecl),
  GlobalVar(GlobalVar),
}

impl PrintSource for GlobalExp {
  fn print_source(&self) -> String {
    match self {
      Self::ClassDec(class_decl) => class_decl.print_source(),
      Self::GlobalVar(global_val) => global_val.print_source(),
    }
  }
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct TypeProgram {
  pub expressions: Vec<GlobalExp>,
}

impl PrintSource for TypeProgram {
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
-> impl Parser<'a, &'a [TypeToken], TypeProgram, extra::Err<Rich<'a, TypeToken>>> {
  let class_parser = parse_class_decl();
  let global_val_parser = parse_global_var();

  let global_exp_parser = class_parser
    .map(|class_decl| GlobalExp::ClassDec(class_decl))
    .or(global_val_parser.map(|global_val| GlobalExp::GlobalVar(global_val)));

  global_exp_parser
    .repeated()
    .collect::<Vec<_>>()
    .map(|f| TypeProgram { expressions: f })
}
