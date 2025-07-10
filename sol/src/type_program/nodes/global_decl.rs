use std::iter::once;

use derive_getters::Getters;
use derive_new::new;

use crate::type_program::{
  nodes::ast_node::{ASTNode, ASTNodeData},
  program_equivalent::ProgramEquivalent,
  types::Type,
};

#[derive(new, Getters, Debug, Clone)]
pub struct GlobalDecl {
  identifier: Box<ASTNode>,
}

impl ProgramEquivalent for GlobalDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.identifier().program_equivalent(b.identifier())
  }
}

impl ASTNodeData for GlobalDecl {
  fn format_source(&self) -> String {
    format!("static {};", self.identifier().format_source())
  }

  fn children(&self) -> Vec<&ASTNode> {
    once(self.identifier.as_ref()).collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    self.identifier().calc_type(_parent_type)
  }
}
