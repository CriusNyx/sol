use logos::Logos;
use serde::Serialize;
use ts_rs::TS;

use crate::helpers::lexer_helpers::{TokenInfo, create_token_info};

#[derive(Logos, Clone, Debug, TS, PartialEq, Serialize)]
pub enum ExpressionToken {
  #[token("[", create_token_info)]
  OpenAngle(TokenInfo),
  #[token("]", create_token_info)]
  ClosedAngle(TokenInfo),

  #[token(".", create_token_info)]
  DotOp(TokenInfo),

  #[regex("[a-zA-Z][a-zA-Z0-9]*", create_token_info)]
  Symbol(TokenInfo),
}

impl ExpressionToken {
  pub fn token_info(&self) -> &TokenInfo {
    match self {
      ExpressionToken::OpenAngle(info) => info,
      ExpressionToken::ClosedAngle(info) => info,
      ExpressionToken::DotOp(info) => info,
      ExpressionToken::Symbol(info) => info,
    }
  }
}
