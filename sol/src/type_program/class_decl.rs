use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{
  GenericParamDecl, MethodDecl, PrintSource, TypeRef, TypeToken, generic_param_set_parser,
  method_decl_parser, type_ref_parser, type_ref_set_parser,
};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct FieldDef<'token> {
  pub field_name: TypeToken<'token>,
  pub type_ref: TypeRef<'token>,
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub enum ClassBodyStatement<'token> {
  FieldDecl(FieldDef<'token>),
  MethodDecl(MethodDecl<'token>),
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct ClassDecl<'token> {
  pub name: TypeToken<'token>,
  pub generic_params: Option<Vec<GenericParamDecl<'token>>>,
  pub inherits: Option<Vec<TypeRef<'token>>>,
  pub body: Option<Vec<ClassBodyStatement<'token>>>,
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

impl<'token> PrintSource for ClassBodyStatement<'token> {
  fn print_source(&self) -> String {
    match self {
      ClassBodyStatement::FieldDecl(field_decl) => format!(
        "{} {};",
        field_decl.type_ref.print_source(),
        field_decl.field_name.to_string()
      ),
      ClassBodyStatement::MethodDecl(method_decl) => method_decl.print_source(),
    }
  }
}

impl<'token> PrintSource for ClassDecl<'token> {
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

pub fn class_decl_parser<'a>()
-> impl Parser<'a, &'a [TypeToken<'a>], ClassDecl<'a>, extra::Err<Rich<'a, TypeToken<'a>>>> {
  let type_parser = type_ref_parser();

  let field_parser = type_parser
    .then(select! { TypeToken::Symbol(sym) => TypeToken::Symbol(sym)  })
    .then_ignore(select! {TypeToken::Semicolon(_)})
    .map(|(type_ref, token)| {
      ClassBodyStatement::FieldDecl(FieldDef {
        field_name: token,
        type_ref,
      })
    });

  let method_parser = method_decl_parser().map(|method| ClassBodyStatement::MethodDecl(method));

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

  let generic_param_decl_parser = generic_param_set_parser();

  let generic_param_parser = generic_param_decl_parser.map(Some).or(empty().to(None));

  let inherit_parser = select! {TypeToken::Colon(_)}
    .then(type_ref_set_parser())
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
