use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::{
    nodes::st_ast::{StAst, ASTNodeData},
    types::Type,
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct UnitDecl {
  unit: Box<StAst>,
}

impl ProgramEquivalent for UnitDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.unit().program_equivalent(b.unit())
  }
}

impl ASTNodeData for UnitDecl {
  fn format_source(&self) -> String {
    format!("({})", self.unit().format_source())
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.unit().as_ref()).collect()
  }

  fn calc_type(&self, parent_type: Option<&Type>) -> (Option<String>, Type) {
    self.unit().calc_type(parent_type)
  }
}
