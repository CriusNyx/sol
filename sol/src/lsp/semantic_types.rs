use derive_getters::Getters;
use derive_new::new;
use serde::Serialize;
use sol_helpers::AllVariants;
use std::fmt;
use ts_rs::{self, TS};

use crate::type_program::type_token::TypeToken;

#[derive(Debug, AllVariants, TS, Clone, Copy, Serialize, PartialEq)]
#[ts(export)]
pub enum SemanticType {
  None,
  Type,
  Variable,
  Keyword,
  Operator,
  Method,
}

#[derive(new, Getters, Debug, TS, Serialize, Clone, PartialEq)]
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

impl TypeToken {
  pub fn semantic_type(&self) -> SemanticType {
    if self.is_keyword() {
      SemanticType::Keyword
    } else if self.is_op() {
      SemanticType::Operator
    } else if let TypeToken::Symbol(_) = self {
      SemanticType::Variable
    } else {
      SemanticType::None
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
