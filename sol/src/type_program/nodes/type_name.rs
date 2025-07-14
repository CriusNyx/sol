use derive_getters::Getters;
use derive_new::new;
use itertools::Itertools;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::nodes::st_ast::{ASTNodeData, StAst},
};

#[derive(new, Getters, Debug, Clone)]
pub struct TypeName {
  path: Vec<StAst>,
}

impl ProgramEquivalent for TypeName {
  fn program_equivalent(&self, other: &Self) -> bool {
    self.path().program_equivalent(other.path())
  }
}

impl ASTNodeData for TypeName {
  fn format_source(&self) -> String {
    self.path().iter().map(|x| x.format_source()).join("::")
  }

  fn children(&self) -> Vec<&StAst> {
    self.path().iter().collect()
  }

  fn calc_type(
    &self,
    parent_type: Option<&crate::type_program::types::Type>,
  ) -> (Option<String>, crate::type_program::types::Type) {
    (
      Some(
        self
          .path()
          .iter()
          .map(|x| x.type_name().unwrap_or("".to_string()))
          .join("::"),
      ),
      parent_type.unwrap().clone(),
    )
  }
}
