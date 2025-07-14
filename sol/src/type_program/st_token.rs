use core::fmt;
use std::fmt::Debug;

use logos::Logos;
use serde::Serialize;
use ts_rs::TS;

use crate::helpers::lexer_helpers::{StTokenInfo, create_token_info};

#[derive(Logos, Clone, Debug, TS, PartialEq, Serialize)]
pub enum StToken {
  // Keywords
  #[token("type", create_token_info)]
  TypeKeyword(StTokenInfo),
  #[token("void", create_token_info)]
  VoidKeyword(StTokenInfo),
  #[token("static", create_token_info)]
  StaticKeyword(StTokenInfo),

  // Symbols
  #[token("::", create_token_info)]
  ScopeOp(StTokenInfo),
  #[token(":", create_token_info)]
  Colon(StTokenInfo),
  #[token(";", create_token_info)]
  Semicolon(StTokenInfo),
  #[token(",", create_token_info)]
  Comma(StTokenInfo),
  #[token("+", create_token_info)]
  AddOp(StTokenInfo),
  #[token("=>", create_token_info)]
  ArrowOp(StTokenInfo),
  #[token("...", create_token_info)]
  Spread(StTokenInfo),

  // Brackets
  #[token("{", create_token_info)]
  OpenCurly(StTokenInfo),
  #[token("}", create_token_info)]
  ClosedCurly(StTokenInfo),
  #[token("(", create_token_info)]
  OpenParen(StTokenInfo),
  #[token(")", create_token_info)]
  ClosedParen(StTokenInfo),
  #[token("[", create_token_info)]
  OpenAngle(StTokenInfo),
  #[token("]", create_token_info)]
  ClosedAngle(StTokenInfo),
  #[token("<", create_token_info)]
  OpenCaret(StTokenInfo),
  #[token(">", create_token_info)]
  ClosedCaret(StTokenInfo),

  // Symbol
  #[regex("[a-zA-Z][a-zA-Z0-9]*", create_token_info)]
  Symbol(StTokenInfo),

  // Whitespace
  #[regex(r"[ \t\f\n]+", logos::skip)]
  Whitespace,
}

impl fmt::Display for StToken {
  fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
    match self {
      Self::Whitespace => write!(f, "<whitespace>"),
      _ => write!(f, "{}", self.get_info().source),
    }
  }
}

impl StToken {
  pub fn get_info_mut(&mut self) -> &mut StTokenInfo {
    match self {
      // Keywords
      Self::TypeKeyword(info) => info,
      Self::VoidKeyword(info) => info,
      Self::StaticKeyword(info) => info,

      // Symbols
      Self::ScopeOp(info) => info,
      Self::Colon(info) => info,
      Self::Semicolon(info) => info,
      Self::Comma(info) => info,
      Self::AddOp(info) => info,
      Self::ArrowOp(info) => info,
      Self::Spread(info) => info,

      // Brackets
      Self::OpenCurly(info) => info,
      Self::ClosedCurly(info) => info,
      Self::OpenParen(info) => info,
      Self::ClosedParen(info) => info,
      Self::OpenAngle(info) => info,
      Self::ClosedAngle(info) => info,
      Self::OpenCaret(info) => info,
      Self::ClosedCaret(info) => info,

      Self::Symbol(info) => info,
      Self::Whitespace => panic!(),
    }
  }

  pub fn get_info(&self) -> &StTokenInfo {
    match self {
      // Keywords
      Self::TypeKeyword(info) => info,
      Self::VoidKeyword(info) => info,
      Self::StaticKeyword(info) => info,

      // Symbols
      Self::ScopeOp(info) => info,
      Self::Colon(info) => info,
      Self::Semicolon(info) => info,
      Self::Comma(info) => info,
      Self::AddOp(info) => info,
      Self::ArrowOp(info) => info,
      Self::Spread(info) => info,

      // Brackets
      Self::OpenCurly(info) => info,
      Self::ClosedCurly(info) => info,
      Self::OpenParen(info) => info,
      Self::ClosedParen(info) => info,
      Self::OpenAngle(info) => info,
      Self::ClosedAngle(info) => info,
      Self::OpenCaret(info) => info,
      Self::ClosedCaret(info) => info,

      Self::Symbol(info) => info,

      Self::Whitespace => panic!(),
    }
  }

  pub fn is_keyword(&self) -> bool {
    match self {
      StToken::TypeKeyword(_) | StToken::VoidKeyword(_) | StToken::StaticKeyword(_) => true,
      _ => false,
    }
  }

  pub fn is_op(&self) -> bool {
    match self {
      StToken::AddOp(_) | StToken::Spread(_) => true,
      _ => false,
    }
  }
}
