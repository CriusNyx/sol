use logos::{Lexer, Logos, Span};
use serde::Serialize;
use ts_rs::TS;

#[derive(Clone, Debug, PartialEq, Serialize, TS)]
pub struct TokenInfo {
  pub span: Span,
  pub source: Box<str>,
  pub index: i32,
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
