use derive_new::new;
use logos::{Lexer, Logos};
use serde::Serialize;
use std::ops::Range;
use strum::IntoDiscriminant;
use strum_macros::EnumDiscriminants;
use ts_rs::TS;

use crate::helpers::program_equivalent::ProgramEquivalent;

#[derive(Logos, Clone, Debug, TS, PartialEq, Serialize, EnumDiscriminants)]
pub enum ExpressionToken {
  #[token("(", create_expression_token_info)]
  OpenParen(ExpressionTokenInfo),
  #[token(")", create_expression_token_info)]
  ClosedParen(ExpressionTokenInfo),
  #[token("[", create_expression_token_info)]
  OpenAngle(ExpressionTokenInfo),
  #[token("]", create_expression_token_info)]
  ClosedAngle(ExpressionTokenInfo),

  #[token(".", create_expression_token_info)]
  DotOp(ExpressionTokenInfo),

  #[regex("[a-zA-Z][a-zA-Z0-9]*", create_expression_token_info)]
  Symbol(ExpressionTokenInfo),

  #[token("+", create_expression_token_info)]
  AddOp(ExpressionTokenInfo),
  #[token("-", create_expression_token_info)]
  MinusOp(ExpressionTokenInfo),
  #[token("*", create_expression_token_info)]
  MulOp(ExpressionTokenInfo),
  #[token("\\", create_expression_token_info)]
  DivOp(ExpressionTokenInfo),

  #[regex(r"[ \t\f\n]+", create_expression_token_info)]
  Whitespace(ExpressionTokenInfo),
}

impl ExpressionTokenImpl for ExpressionToken {
  fn get_token_info(&self) -> &ExpressionTokenInfo {
    match self {
      ExpressionToken::OpenParen(info) => info,
      ExpressionToken::ClosedParen(info) => info,
      ExpressionToken::OpenAngle(info) => info,
      ExpressionToken::ClosedAngle(info) => info,

      ExpressionToken::DotOp(info) => info,
      ExpressionToken::Symbol(info) => info,

      ExpressionToken::AddOp(info) => info,
      ExpressionToken::MinusOp(info) => info,
      ExpressionToken::MulOp(info) => info,
      ExpressionToken::DivOp(info) => info,

      ExpressionToken::Whitespace(info) => info,
    }
  }
}

#[derive(new, Clone, Debug, PartialEq, Serialize, TS)]
pub struct ExpressionTokenInfo {
  pub span: Range<usize>,
  pub source: String,
  pub index: i32,
}

pub trait ExpressionTokenImpl {
  fn get_token_info(&self) -> &ExpressionTokenInfo;
}

impl ExpressionTokenImpl for ExpressionTokenInfo {
  fn get_token_info(&self) -> &ExpressionTokenInfo {
    self
  }
}

pub fn create_expression_token_info<'lexer, T: Logos<'lexer, Source = str>>(
  lexer: &mut Lexer<'lexer, T>,
) -> ExpressionTokenInfo {
  ExpressionTokenInfo {
    span: lexer.span(),
    source: lexer.slice().into(),
    index: -1,
  }
}

pub fn lex_expression(source: &str) -> Result<Vec<ExpressionToken>, ()> {
  ExpressionToken::lexer(source)
    .filter(|x| {
      x.as_ref().map_or(false, |x| {
        x.discriminant() != ExpressionTokenDiscriminants::Whitespace
      })
    })
    .collect::<Result<Vec<_>, _>>()
}

impl ProgramEquivalent for ExpressionTokenInfo {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.source == b.source
  }
}

impl ProgramEquivalent for ExpressionToken {
  fn program_equivalent(&self, other: &Self) -> bool {
    self.discriminant() == other.discriminant()
      && self
        .get_token_info()
        .program_equivalent(other.get_token_info())
  }
}
