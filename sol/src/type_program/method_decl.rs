use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{
  GenericParamDecl, PrintSource, TypeRef, TypeToken, generic_param_set_parser, type_ref_parser,
};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct MethodParam {
  pub type_ref: TypeRef,
  pub variadic: bool,
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct MethodDecl {
  pub is_static: bool,
  pub name: TypeToken,
  pub generic_params: Option<Vec<GenericParamDecl>>,
  pub return_type: Option<TypeRef>,
  pub param_types: Vec<MethodParam>,
}

impl PrintSource for MethodParam {
  fn print_source(&self) -> String {
    if self.variadic {
      format!("... {}", self.type_ref.print_source())
    } else {
      self.type_ref.print_source()
    }
  }
}

impl PrintSource for MethodDecl {
  fn print_source(&self) -> String {
    let return_type_string = match &self.return_type {
      Some(val) => val.print_source(),
      None => "void".to_string(),
    };

    format!(
      "{}{}{}({}): {};",
      if self.is_static { "static " } else { "" },
      self.name.to_string(),
      self.generic_params.print_source(),
      self
        .param_types
        .iter()
        .map(|x| x.print_source())
        .collect::<Vec<_>>()
        .join(", "),
      return_type_string
    )
  }
}

pub fn method_decl_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], MethodDecl, extra::Err<Rich<'a, TypeToken>>> {
  let static_parser = select! {TypeToken::StaticKeyword(_) => true}.or(empty().to(false));

  let type_ref_parser = type_ref_parser();

  let return_type_parser = type_ref_parser
    .clone()
    .map(Some)
    .or(select! {TypeToken::VoidKeyword(_)}.to(None));

  let generic_param_decl_parser = generic_param_set_parser();

  let generic_param_parser = generic_param_decl_parser.map(Some).or(empty().to(None));

  let method_param_parser = select! {TypeToken::Spread(_)}
    .then(type_ref_parser.clone())
    .map(|(_, type_ref)| MethodParam {
      type_ref,
      variadic: true,
    })
    .or(type_ref_parser.map(|type_ref| MethodParam {
      type_ref,
      variadic: false,
    }));

  let param_body_parser = method_param_parser
    .separated_by(select_ref! {TypeToken::Comma(_)})
    .collect::<Vec<_>>()
    .delimited_by(
      select! {TypeToken::OpenParen(_)},
      select! {TypeToken::ClosedParen(_)},
    );

  let method_parser = static_parser
    .then(select! {TypeToken::Symbol(sym) => TypeToken::Symbol(sym)})
    .then(generic_param_parser)
    .then(param_body_parser)
    .then_ignore(select! {TypeToken::Colon(_)})
    .then(return_type_parser)
    .then_ignore(select!(TypeToken::Semicolon(_)))
    .map(
      |((((is_static, token), generic_params), param_types), return_type)| MethodDecl {
        is_static,
        name: token,
        return_type,
        generic_params,
        param_types,
      },
    );

  method_parser
}
