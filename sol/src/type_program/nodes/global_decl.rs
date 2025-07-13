use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::{
    nodes::st_ast::{ASTNodeData, StAst},
    types::Type,
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct GlobalDecl {
  identifier: Box<StAst>,
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

  fn children(&self) -> Vec<&StAst> {
    once(self.identifier.as_ref()).collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    self.identifier().calc_type(_parent_type)
  }
}
