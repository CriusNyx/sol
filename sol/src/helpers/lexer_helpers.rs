use enum_dispatch::enum_dispatch;
use logos::{Lexer, Logos, Span};
use serde::Serialize;
use ts_rs::TS;

#[derive(Clone, Debug, PartialEq, Serialize, TS)]
pub struct StTokenInfo {
  pub span: Span,
  pub source: Box<str>,
  pub index: i32,
}

#[enum_dispatch]
pub trait TokenInfoImpl {
  fn get_token_info(&self) -> &StTokenInfo;
}

impl TokenInfoImpl for StTokenInfo {
  fn get_token_info(&self) -> &StTokenInfo {
    self
  }
}

pub fn create_token_info<'lexer, T: Logos<'lexer, Source = str>>(
  lexer: &mut Lexer<'lexer, T>,
) -> StTokenInfo {
  StTokenInfo {
    span: lexer.span(),
    source: lexer.slice().into(),
    index: -1,
  }
}
