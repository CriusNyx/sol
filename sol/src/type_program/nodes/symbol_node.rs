use derive_getters::Getters;
use derive_new::new;

use crate::type_program::{
  nodes::ast_node::{ASTNode, ASTNodeData},
  program_equivalent::ProgramEquivalent,
  type_system::Type,
};

#[derive(new, Getters, Debug, Clone)]
pub struct SymbolNode {
  name: String,
}

impl ProgramEquivalent for SymbolNode {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.name == b.name
  }
}

impl ASTNodeData for SymbolNode {
  fn format_source(&self) -> String {
    self.name().to_string()
  }

  fn children(&self) -> Vec<&ASTNode> {
    vec![]
  }

  fn calc_type(&self, parent_type: Option<&Type>) -> (Option<String>, Type) {
    (Some(self.name().to_string()), parent_type.unwrap().clone())
  }
}
