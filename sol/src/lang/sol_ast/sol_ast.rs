use derive_more::From;
use std::rc::Rc;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  lang::sol_ast::{
    deref_expression::ReferenceChain, expression_set::ExpressionSet,
    symbol_expression::SymbolExpression,
  },
};

#[derive(From, Clone, Debug)]
pub enum SolAST {
  ExpressionSet(ExpressionSet),
  SymbolExpression(SymbolExpression),
  ReferenceChain(ReferenceChain),
}

impl ProgramEquivalent for SolAST {
  fn program_equivalent(&self, other: &Self) -> bool {
    match (self, other) {
      (SolAST::SymbolExpression(a), SolAST::SymbolExpression(b)) => a.program_equivalent(b),
      (SolAST::ReferenceChain(a), SolAST::ReferenceChain(b)) => a.program_equivalent(b),
      (SolAST::ExpressionSet(a), SolAST::ExpressionSet(b)) => a.program_equivalent(b),
      _ => false,
    }
  }
}

pub trait ToRc<T> {
  fn to_rc(self) -> Rc<T>;
}

impl<T: Into<SolAST> + Clone> ToRc<SolAST> for T {
  fn to_rc(self) -> Rc<SolAST> {
    Rc::new(self.clone().into())
  }
}
