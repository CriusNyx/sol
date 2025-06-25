use core::fmt;

use logos::{Lexer, Logos, Span};
use serde::Serialize;
use ts_rs::TS;

#[derive(Clone, Debug, PartialEq, Serialize, TS)]
pub struct TokenInfo {
  pub span: Span,
  pub source: Box<str>,
  pub index: i32,
}

fn create_token_info<'lexer>(lexer: &mut Lexer<'lexer, TypeToken>) -> TokenInfo {
  TokenInfo {
    span: lexer.span(),
    source: lexer.slice().into(),
    index: -1,
  }
}

#[derive(Logos, Clone, Debug, TS, PartialEq, Serialize)]
pub enum TypeToken {
  // Keywords
  #[token("type", create_token_info)]
  TypeKeyword(TokenInfo),
  #[token("void", create_token_info)]
  VoidKeyword(TokenInfo),
  #[token("static", create_token_info)]
  StaticKeyword(TokenInfo),

  // Symbols
  #[token(":", create_token_info)]
  Colon(TokenInfo),
  #[token(";", create_token_info)]
  Semicolon(TokenInfo),
  #[token(",", create_token_info)]
  Comma(TokenInfo),
  #[token("+", create_token_info)]
  AddOpp(TokenInfo),
  #[token("...", create_token_info)]
  Spread(TokenInfo),

  // Brackets
  #[token("{", create_token_info)]
  OpenCurly(TokenInfo),
  #[token("}", create_token_info)]
  ClosedCurly(TokenInfo),
  #[token("(", create_token_info)]
  OpenParen(TokenInfo),
  #[token(")", create_token_info)]
  ClosedParen(TokenInfo),
  #[token("[", create_token_info)]
  OpenAngle(TokenInfo),
  #[token("]", create_token_info)]
  ClosedAngle(TokenInfo),
  #[token("<", create_token_info)]
  OpenCaret(TokenInfo),
  #[token(">", create_token_info)]
  ClosedCaret(TokenInfo),

  // Symbol
  #[regex("[a-zA-Z][a-zA-Z0-9]*", create_token_info)]
  Symbol(TokenInfo),

  // Whitespace
  #[regex(r"[ \t\f\n]+", logos::skip)]
  Whitespace,
}

impl fmt::Display for TypeToken {
  fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
    match self {
      Self::Whitespace => write!(f, "<whitespace>"),
      _ => write!(f, "{}", self.get_info().source),
    }
  }
}

impl TypeToken {
  pub fn get_info_mut(&mut self) -> &mut TokenInfo {
    match self {
      // Keywords
      Self::TypeKeyword(info) => info,
      Self::VoidKeyword(info) => info,
      Self::StaticKeyword(info) => info,

      // Symbols
      Self::Colon(info) => info,
      Self::Semicolon(info) => info,
      Self::Comma(info) => info,
      Self::AddOpp(info) => info,
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

  pub fn get_info(&self) -> &TokenInfo {
    match self {
      // Keywords
      Self::TypeKeyword(info) => info,
      Self::VoidKeyword(info) => info,
      Self::StaticKeyword(info) => info,

      // Symbols
      Self::Colon(info) => info,
      Self::Semicolon(info) => info,
      Self::Comma(info) => info,
      Self::AddOpp(info) => info,
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
      TypeToken::TypeKeyword(_) | TypeToken::VoidKeyword(_) | TypeToken::StaticKeyword(_) => true,
      _ => false,
    }
  }

  pub fn is_op(&self) -> bool {
    match self {
      TypeToken::AddOpp(_) | TypeToken::Spread(_) => true,
      _ => false,
    }
  }
}
