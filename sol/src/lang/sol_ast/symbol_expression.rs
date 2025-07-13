use derive_getters::Getters;
use derive_new::new;

use crate::{
  helpers::program_equivalent::ProgramEquivalent, lang::expression_token::ExpressionToken,
};

#[derive(new, Getters, Clone, Debug)]
pub struct SymbolExpression {
  token: ExpressionToken,
}

impl ProgramEquivalent for SymbolExpression {
  fn program_equivalent(&self, other: &Self) -> bool {
    self.token().program_equivalent(other.token())
  }
}
