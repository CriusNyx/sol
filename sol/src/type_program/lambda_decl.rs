use chumsky::{Parser, error::Rich, extra, prelude::empty, select};
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{
  GenericParamDecl, MethodParamDecl, PrintSource, TypeRef, TypeToken, generic_param_set_parser,
  param_set_parser, return_type_parser,
};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct LambdaDecl {
  pub param_types: Vec<MethodParamDecl>,
  pub generic_params: Option<Vec<GenericParamDecl>>,
  pub return_type: Option<Box<TypeRef>>,
}

pub fn lambda_parser<'a>(
  type_ref_parser: impl Parser<'a, &'a [TypeToken], TypeRef, extra::Err<Rich<'a, TypeToken>>> + Clone,
) -> impl Parser<'a, &'a [TypeToken], LambdaDecl, extra::Err<Rich<'a, TypeToken>>> + Clone {
  let generic_parser = generic_param_set_parser(type_ref_parser.clone())
    .map(Some)
    .or(empty().to(None));

  param_set_parser(type_ref_parser.clone())
    .then(generic_parser)
    .then_ignore(select! { TypeToken::ArrowOp(_) => () })
    .then(return_type_parser(type_ref_parser))
    .map(|((a, b), c)| LambdaDecl {
      param_types: a,
      generic_params: b,
      return_type: c.map(|x| Box::new(x)),
    })
}

impl PrintSource for LambdaDecl {
  fn print_source(&self) -> String {
    format!(
      "{}{} => {}",
      self.param_types.print_source(),
      self.generic_params.print_source(),
      self
        .return_type
        .as_ref()
        .map(|x| x.as_ref().print_source())
        .unwrap_or("void".to_string())
    )
  }
}
