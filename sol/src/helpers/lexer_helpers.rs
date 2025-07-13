use enum_dispatch::enum_dispatch;
use logos::{Lexer, Logos, Span};
use serde::Serialize;
use ts_rs::TS;

#[derive(Clone, Debug, PartialEq, Serialize, TS)]
pub struct TokenInfo {
  pub span: Span,
  pub source: Box<str>,
  pub index: i32,
}

#[enum_dispatch]
pub trait TokenInfoImpl {
  fn get_token_info(&self) -> &TokenInfo;
}

impl TokenInfoImpl for TokenInfo {
  fn get_token_info(&self) -> &TokenInfo {
    self
  }
}

pub fn create_token_info<'lexer, T: Logos<'lexer, Source = str>>(
  lexer: &mut Lexer<'lexer, T>,
) -> TokenInfo {
  TokenInfo {
    span: lexer.span(),
    source: lexer.slice().into(),
    index: -1,
  }
}
