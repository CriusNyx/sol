use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::{
    nodes::st_ast::{ASTNodeData, StAst},
    types::*,
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct ArrayDecl {
  arity: usize,
  type_decl: Box<StAst>,
}

impl ProgramEquivalent for ArrayDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.arity() == b.arity() && self.type_decl().program_equivalent(&b.type_decl())
  }
}

impl ASTNodeData for ArrayDecl {
  fn format_source(&self) -> String {
    format!(
      "{}[{}]",
      self.type_decl().as_ref().data().format_source(),
      (0..*self.arity())
        .into_iter()
        .map(|_| "")
        .collect::<Vec<_>>()
        .join(","),
    )
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.type_decl().as_ref()).collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    (
      None,
      ArrayType::new(*self.arity(), self.type_decl().calc_type(None).1.to_rc()).into(),
    )
  }
}
