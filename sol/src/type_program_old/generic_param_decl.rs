use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program_old::{PrintSource, TypeRefAST, TypeToken, type_ref_set_parser};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct GenericParamDecl {
  pub name: TypeToken,
  pub inherits: Option<Vec<TypeRefAST>>,
}

impl PrintSource for GenericParamDecl {
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

impl PrintSource for Option<Vec<GenericParamDecl>> {
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

pub fn generic_param_set_parser<'a>(
  type_ref_parser: impl Parser<'a, &'a [TypeToken], TypeRefAST, extra::Err<Rich<'a, TypeToken>>> + Clone,
) -> impl Parser<'a, &'a [TypeToken], Vec<GenericParamDecl>, extra::Err<Rich<'a, TypeToken>>> + Clone
{
  let no_inherits_parser =
    select! {TypeToken::Symbol(sym) => TypeToken::Symbol(sym)}.map(|token| GenericParamDecl {
      name: token,
      inherits: None,
    });

  let inherits_parser = select! {TypeToken::Symbol(sym) => TypeToken::Symbol(sym)}
    .then_ignore(select! {TypeToken::Colon(_)})
    .then(type_ref_set_parser(type_ref_parser))
    .map(|(token, inherits)| GenericParamDecl {
      name: token,
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
