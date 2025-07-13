use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::{
    nodes::ast_node::{ASTNode, ASTNodeData},
    types::{MethodParamType, Type, TypeImpl},
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct MethodParamDecl {
  type_ref: Box<ASTNode>,
  variadic: bool,
}

impl ProgramEquivalent for MethodParamDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    *self.variadic() == *b.variadic() && self.type_ref().program_equivalent(b.type_ref())
  }
}

impl ASTNodeData for MethodParamDecl {
  fn format_source(&self) -> String {
    format!(
      "{}{}",
      if *self.variadic() { "..." } else { "" },
      self.type_ref().format_source()
    )
  }

  fn children(&self) -> Vec<&ASTNode> {
    once(self.type_ref().as_ref()).collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let output = self.type_ref().calc_type(None);

    (
      output.0,
      MethodParamType::new(output.1.to_rc(), *self.variadic()).into(),
    )
  }
}
