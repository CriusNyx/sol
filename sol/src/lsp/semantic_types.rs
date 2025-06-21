use crate::type_program::TypeToken;
use serde::Serialize;
use sol_helpers::AllVariants;
use std::fmt;
use ts_rs::{self, TS};

#[derive(Debug, AllVariants, TS, Clone, Copy, Serialize)]
#[ts(export)]
pub enum SemanticType {
  None,
  Type,
  Variable,
  Keyword,
  Operator,
  Method,
}

#[derive(Debug, TS, Serialize)]
#[ts(export)]
pub struct SemanticToken {
  pub token_type: SemanticType,
  pub start: u32,
  pub end: u32,
  pub len: u32,
}

impl fmt::Display for SemanticType {
  fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
    write!(f, "{:?}", self)
  }
}

impl<'a> TypeToken<'a> {
  pub fn semantic_type(&self) -> SemanticType {
    match self {
      TypeToken::TypeKeyword(_) | TypeToken::VoidKeyword(_) => SemanticType::Keyword,
      TypeToken::AddOpp(_) | TypeToken::Spread(_) => SemanticType::Operator,
      TypeToken::Symbol(_) => SemanticType::Variable,
      _ => SemanticType::None,
    }
  }

  pub fn into_semantic_token(&self) -> SemanticToken {
    let span = &self.get_info().span;
    let start = span.start as u32;
    let end = span.end as u32;
    return SemanticToken {
      token_type: self.semantic_type(),
      start,
      end: end,
      len: end - start,
    };
  }
}
