use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{LambdaDecl, PrintSource, TypeToken, lambda_parser};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct SymTypeRef {
  pub name: TypeToken,
  pub params: Option<Vec<TypeRef>>,
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct ArrayTypeRef {
  pub arity: u16,
  pub array_type: TypeRef,
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub enum TypeRef {
  ArrayTypeRef(Box<ArrayTypeRef>),
  SymTypeRef(SymTypeRef),
  LambdaDecl(LambdaDecl),
}

impl Into<TypeRef> for ArrayTypeRef {
  fn into(self) -> TypeRef {
    TypeRef::ArrayTypeRef(Box::new(self))
  }
}

impl Into<TypeRef> for SymTypeRef {
  fn into(self) -> TypeRef {
    TypeRef::SymTypeRef(self)
  }
}

impl Into<TypeRef> for LambdaDecl {
  fn into(self) -> TypeRef {
    TypeRef::LambdaDecl(self)
  }
}

impl PrintSource for SymTypeRef {
  fn print_source(&self) -> String {
    match &self.params {
      Some(params) => format!(
        "{}<{}>",
        self.name.to_string(),
        params
          .into_iter()
          .map(|f| f.print_source())
          .collect::<Vec<_>>()
          .join(", ")
      ),
      None => self.name.to_string(),
    }
  }
}

impl PrintSource for ArrayTypeRef {
  fn print_source(&self) -> String {
    format!(
      "{}[{}]",
      self.array_type.print_source(),
      vec![""; self.arity.into()].join(", ")
    )
  }
}

impl PrintSource for TypeRef {
  fn print_source(&self) -> String {
    match self {
      TypeRef::ArrayTypeRef(arr) => arr.print_source(),
      TypeRef::SymTypeRef(sym) => sym.print_source(),
      TypeRef::LambdaDecl(lambda) => lambda.print_source(),
    }
  }
}

pub fn type_ref_set_parser<'a>(
  type_ref_parser: impl Parser<'a, &'a [TypeToken], TypeRef, extra::Err<Rich<'a, TypeToken>>> + Clone,
) -> impl Parser<'a, &'a [TypeToken], Vec<TypeRef>, extra::Err<Rich<'a, TypeToken>>> + Clone {
  type_ref_parser
    .separated_by(select_ref! { TypeToken::AddOp(_) })
    .collect::<Vec<_>>()
}

pub fn type_ref_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], TypeRef, extra::Err<Rich<'a, TypeToken>>> + Clone {
  recursive(|type_ref| {
    let params_set = type_ref
      .clone()
      .separated_by(select_ref! {TypeToken::Comma(_)})
      .collect::<Vec<_>>()
      .map(|params| params);

    let type_params = params_set
      .delimited_by(
        select_ref! {TypeToken::OpenCaret(_)},
        select_ref! {TypeToken::ClosedCaret(_)},
      )
      .map(|params| Some(params))
      .or(empty().to(None));

    let sym_type_ref = select! {TypeToken::Symbol(info) => TypeToken::Symbol(info)}
      .then(type_params)
      .map(|(token, params)| {
        TypeRef::SymTypeRef(SymTypeRef {
          name: token,
          params,
        })
      });

    let array_arity_decl = select_ref! {TypeToken::OpenAngle(_)}
      .then(
        empty()
          .separated_by(select_ref! {TypeToken::Comma(_)})
          .collect::<Vec<_>>(),
      )
      .then_ignore(select_ref! {TypeToken::ClosedAngle(_)})
      .map(|(_, arr)| arr.len() as u16);

    let array_decl = sym_type_ref
      .clone()
      .then(array_arity_decl.repeated().collect::<Vec<_>>())
      .map(|(type_ref, arity)| {
        let arr = arity as Vec<_>;
        arr.into_iter().fold(type_ref, |prev, curr| {
          TypeRef::ArrayTypeRef(Box::new(ArrayTypeRef {
            arity: curr,
            array_type: prev,
          }))
        })
      });

    array_decl.or(lambda_parser(type_ref).map(|x| x.into()))
  })
}
