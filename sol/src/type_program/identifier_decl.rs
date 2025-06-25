use chumsky::{Parser, error::Rich, extra, select};
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{PrintSource, TypeRef, TypeToken, type_ref_parser};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct Identifier<'token> {
  pub identifier_name: TypeToken<'token>,
  pub type_ref: TypeRef<'token>,
}

impl<'token> PrintSource for Identifier<'token> {
  fn print_source(&self) -> String {
    format!(
      "{}: {}",
      self.identifier_name.to_string(),
      self.type_ref.print_source()
    )
  }
}

pub fn parse_identifier<'a>()
-> impl Parser<'a, &'a [TypeToken<'a>], Identifier<'a>, extra::Err<Rich<'a, TypeToken<'a>>>> {
  return select! {TypeToken::Symbol(sym) => TypeToken::Symbol(sym)}
    .then_ignore(select! {TypeToken::Colon(_)})
    .then(type_ref_parser())
    .map(|(sym, type_ref)| Identifier {
      identifier_name: sym,
      type_ref,
    });
}
