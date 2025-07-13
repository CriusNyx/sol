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
pub struct IdentifierDecl {
  name: Box<StAst>,
  type_ref: Box<StAst>,
}

impl ProgramEquivalent for IdentifierDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.name().program_equivalent(b.name()) && self.type_ref().program_equivalent(&b.type_ref())
  }
}

impl ASTNodeData for IdentifierDecl {
  fn format_source(&self) -> String {
    format!(
      "{}: {}",
      self.name().format_source(),
      self.type_ref().format_source()
    )
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.name().as_ref())
      .chain(once(self.type_ref().as_ref()))
      .collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let output = self.type_ref().calc_type(None).1;

    self.name().calc_type(Some(&output));

    (Some(self.name().sym_name().unwrap()), output)
  }
}
