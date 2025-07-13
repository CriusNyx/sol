use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::{
    nodes::st_ast::{ASTNodeData, StAst},
    types::{FieldType, Type, TypeImpl},
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct FieldDecl {
  identifier: Box<StAst>,
  is_static: bool,
}

impl ProgramEquivalent for FieldDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    *self.is_static() == *b.is_static() && self.identifier().program_equivalent(b.identifier())
  }
}

impl ASTNodeData for FieldDecl {
  fn format_source(&self) -> String {
    format!(
      "{}{};",
      if *self.is_static() { "static " } else { "" },
      self.identifier().format_source()
    )
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.identifier().as_ref()).collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let ident_type = self.identifier().calc_type(None);
    (
      ident_type.0,
      FieldType::new(ident_type.1.to_rc(), *self.is_static()).into(),
    )
  }
}
