use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{PrintSource, TypeRef, TypeToken, type_ref_set_parser};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct GenericParamDecl<'token> {
  pub name: &'token str,
  pub inherits: Option<Vec<TypeRef<'token>>>,
}

impl<'token> PrintSource for GenericParamDecl<'token> {
  fn print_source(&self) -> String {
    match &self.inherits {
      Some(val) => format!(
        "{}: {}",
        self.name,
        val
          .iter()
          .map(|f| f.print_source())
          .collect::<Vec<_>>()
          .join(" + ")
      ),
      None => self.name.to_string(),
    }
  }
}

impl<'token> PrintSource for Option<Vec<GenericParamDecl<'token>>> {
  fn print_source(&self) -> String {
    match &self {
      Some(val) => format!(
        "<{}>",
        val
          .iter()
          .map(|x| x.print_source())
          .collect::<Vec<_>>()
          .join(", ")
      ),
      None => format!(""),
    }
  }
}

pub fn generic_param_set_parser<'a>()
-> impl Parser<'a, &'a [TypeToken<'a>], Vec<GenericParamDecl<'a>>, extra::Err<Rich<'a, TypeToken<'a>>>>
{
  let no_inherits_parser = select! {TypeToken::Symbol(sym) => sym}.map(|token| GenericParamDecl {
    name: token.source,
    inherits: None,
  });

  let inherits_parser = select! {TypeToken::Symbol(sym) => sym}
    .then_ignore(select! {TypeToken::Colon(_)})
    .then(type_ref_set_parser())
    .map(|(token, inherits)| GenericParamDecl {
      name: token.source,
      inherits: Some(inherits),
    });

  let generic_param_parser = inherits_parser.or(no_inherits_parser);

  let param_set_parser = generic_param_parser
    .separated_by(select! {TypeToken::Comma(_)})
    .collect::<Vec<_>>()
    .delimited_by(
      select! {TypeToken::OpenCaret(_)},
      select! {TypeToken::ClosedCaret(_)},
    );

  param_set_parser
}
