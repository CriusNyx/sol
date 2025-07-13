use derive_getters::Getters;
use derive_new::new;
use std::rc::Rc;

use crate::{helpers::program_equivalent::ProgramEquivalent, lang::sol_ast::sol_ast::SolAST};

#[derive(new, Getters, Clone, Debug)]
pub struct ExpressionSet {
  expressions: Vec<Rc<SolAST>>,
}

impl ProgramEquivalent for ExpressionSet {
  fn program_equivalent(&self, other: &Self) -> bool {
    self.expressions().program_equivalent(other.expressions())
  }
}
