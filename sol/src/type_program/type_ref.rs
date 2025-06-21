use chumsky::prelude::*;
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::{PrintSource, TokenSource, TypeToken};

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct SymTypeRef<'token> {
  pub name: TypeToken<'token>,
  pub params: Option<Vec<TypeRef<'token>>>,
  pub tokens: TokenSource<'token>,
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct ArrayTypeRef<'token> {
  pub arity: u16,
  pub array_type: TypeRef<'token>,
  pub tokens: TokenSource<'token>,
}

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub enum TypeRef<'token> {
  ArrayTypeRef(Box<ArrayTypeRef<'token>>),
  SymTypeRef(SymTypeRef<'token>),
}

impl<'a> PrintSource for SymTypeRef<'a> {
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

impl<'token> PrintSource for ArrayTypeRef<'token> {
  fn print_source(&self) -> String {
    format!(
      "{}[{}]",
      self.array_type.print_source(),
      vec![""; self.arity.into()].join(", ")
    )
  }
}

impl<'token> PrintSource for TypeRef<'token> {
  fn print_source(&self) -> String {
    match self {
      TypeRef::ArrayTypeRef(arr) => arr.print_source(),
      TypeRef::SymTypeRef(sym) => sym.print_source(),
    }
  }
}

pub fn type_ref_set_parser<'a>()
-> impl Parser<'a, &'a [TypeToken<'a>], Vec<TypeRef<'a>>, extra::Err<Rich<'a, TypeToken<'a>>>> {
  type_ref_parser()
    .separated_by(select_ref! { TypeToken::AddOpp(_) })
    .collect::<Vec<_>>()
}

pub fn type_ref_parser<'a>()
-> impl Parser<'a, &'a [TypeToken<'a>], TypeRef<'a>, extra::Err<Rich<'a, TypeToken<'a>>>> + Clone {
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
      .map_with(|(token, params), extra| {
        TypeRef::SymTypeRef(SymTypeRef {
          name: token,
          params,
          tokens: TokenSource::from_extra(extra),
        })
      });

    let array_arity_decl = select_ref! {TypeToken::OpenAngle(_)}
      .then(
        empty()
          .separated_by(select_ref! {TypeToken::Comma(_)})
          .collect::<Vec<_>>(),
      )
      .then_ignore(select_ref! {TypeToken::ClosedAngle(_)})
      .map_with(|(_, body), extra| (body.iter().count() as u16, TokenSource::from_extra(extra)));

    let array_decl = sym_type_ref
      .clone()
      .then(array_arity_decl.repeated().collect::<Vec<_>>())
      .map(|(type_ref, arity)| {
        let arr = arity as Vec<(_, _)>;
        arr.into_iter().fold(type_ref, |prev, (curr, tokens)| {
          TypeRef::ArrayTypeRef(Box::new(ArrayTypeRef {
            arity: curr,
            array_type: prev,
            tokens,
          }))
        })
      });

    array_decl
  })
}
