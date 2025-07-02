use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{
  GenericParamDecl, Identifier, MethodDecl, PrintSource, TypeRef, TypeToken,
  generic_param_set_parser, method_decl_parser, parse_identifier, type_ref_parser,
  type_ref_set_parser,
};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct FieldDef {
  pub is_static: bool,
  pub identifier: Identifier,
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub enum ClassBodyStatement {
  FieldDecl(FieldDef),
  MethodDecl(MethodDecl),
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct ClassDecl {
  pub name: TypeToken,
  pub generic_params: Option<Vec<GenericParamDecl>>,
  pub inherits: Option<Vec<TypeRef>>,
  pub body: Option<Vec<ClassBodyStatement>>,
}

fn print_class_body(body: &Option<Vec<ClassBodyStatement>>) -> String {
  match body {
    Some(body) => {
      let body_text = body
        .iter()
        .map(|x| format!("  {}", x.print_source()))
        .collect::<Vec<_>>()
        .join("\n");
      format!(" {{\n{}\n}}", body_text)
    }
    _ => ";".to_string(),
  }
}

fn print_inherits(inherits: &Option<Vec<TypeRef>>) -> String {
  match inherits {
    Some(val) => format!(
      ": {}",
      val
        .iter()
        .map(|x| x.print_source())
        .collect::<Vec<_>>()
        .join(" + ")
    ),
    None => "".to_string(),
  }
}

impl PrintSource for ClassBodyStatement {
  fn print_source(&self) -> String {
    match self {
      ClassBodyStatement::FieldDecl(field_decl) => {
        format!(
          "{}{};",
          if field_decl.is_static { "static " } else { "" },
          field_decl.identifier.print_source()
        )
      }
      ClassBodyStatement::MethodDecl(method_decl) => method_decl.print_source(),
    }
  }
}

impl PrintSource for ClassDecl {
  fn print_source(&self) -> String {
    format!(
      "type {}{}{}{}",
      self.name,
      self.generic_params.print_source(),
      print_inherits(&self.inherits),
      print_class_body(&self.body)
    )
  }
}

pub fn parse_class_decl<'a>()
-> impl Parser<'a, &'a [TypeToken], ClassDecl, extra::Err<Rich<'a, TypeToken>>> + Clone {
  let static_parser = select! {TypeToken::StaticKeyword(_) => true}.or(empty().to(false));

  let field_parser = static_parser
    .then(parse_identifier())
    .then_ignore(select! {TypeToken::Semicolon(_)})
    .map(|(is_static, identifier)| {
      ClassBodyStatement::FieldDecl(FieldDef {
        is_static,
        identifier,
      })
    });

  let method_parser =
    method_decl_parser(type_ref_parser()).map(|method| ClassBodyStatement::MethodDecl(method));

  let statement_parser = field_parser.or(method_parser);

  let body_parser = statement_parser
    .repeated()
    .collect::<Vec<_>>()
    .delimited_by(
      select! {TypeToken::OpenCurly(_)},
      select! {TypeToken::ClosedCurly(_)},
    )
    .map(|f| Some(f))
    .or(select! {TypeToken::Semicolon(_)}.to(None));

  let generic_param_decl_parser = generic_param_set_parser(type_ref_parser());

  let generic_param_parser = generic_param_decl_parser.map(Some).or(empty().to(None));

  let inherit_parser = select! {TypeToken::Colon(_)}
    .then(type_ref_set_parser(type_ref_parser()))
    .map(|(_, set)| Some(set))
    .or(empty().to(None));

  let class_parser = select! {TypeToken::TypeKeyword(_)}
    .then(select! { TypeToken::Symbol(sym) => TypeToken::Symbol(sym) })
    .then(generic_param_parser)
    .then(inherit_parser)
    .then(body_parser)
    .map(
      |((((_, token), generic_params), inherits), body)| ClassDecl {
        name: token,
        generic_params,
        inherits,
        body: body,
      },
    );

  class_parser
}
