use derive_getters::Getters;
use derive_new::new;
use std::rc::Rc;

use crate::{helpers::program_equivalent::ProgramEquivalent, lang::sol_ast::sol_ast::SolAST};

#[derive(new, Getters, Clone, Debug)]
pub struct ReferenceChain {
  chain: Vec<Rc<SolAST>>,
}

impl ProgramEquivalent for ReferenceChain {
  fn program_equivalent(&self, other: &Self) -> bool {
    self.chain().program_equivalent(other.chain())
  }
}
