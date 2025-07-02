use chumsky::{Parser, error::Rich, extra, select};
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{Identifier, PrintSource, TypeToken, parse_identifier};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct GlobalVar {
  pub identifier: Identifier,
}

impl PrintSource for GlobalVar {
  fn print_source(&self) -> String {
    format!("static {};", self.identifier.print_source())
  }
}

pub fn parse_global_var<'a>()
-> impl Parser<'a, &'a [TypeToken], GlobalVar, extra::Err<Rich<'a, TypeToken>>> + Clone {
  select! {TypeToken::StaticKeyword(_)}
    .then(parse_identifier())
    .then_ignore(select! {TypeToken::Semicolon(_)})
    .map(|(_, identifier)| GlobalVar { identifier })
}
