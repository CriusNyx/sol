use chumsky::{Parser, error::Rich, extra, select};
use serde::Serialize;
use ts_rs::TS;

use crate::type_program_old::{PrintSource, TypeRefAST, TypeToken, type_ref_parser};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct Identifier {
  pub identifier_name: TypeToken,
  pub type_decl: TypeRefAST,
}

impl PrintSource for Identifier {
  fn print_source(&self) -> String {
    format!(
      "{}: {}",
      self.identifier_name.to_string(),
      self.type_decl.print_source()
    )
  }
}

pub fn parse_identifier<'a>()
-> impl Parser<'a, &'a [TypeToken], Identifier, extra::Err<Rich<'a, TypeToken>>> + Clone {
  return select! {TypeToken::Symbol(sym) => TypeToken::Symbol(sym)}
    .then_ignore(select! {TypeToken::Colon(_)})
    .then(type_ref_parser())
    .map(|(sym, type_ref)| Identifier {
      identifier_name: sym,
      type_decl: type_ref,
    });
}
