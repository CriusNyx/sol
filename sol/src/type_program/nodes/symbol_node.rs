use crate::type_program::nodes::st_ast::ToAST;
use derive_getters::Getters;
use derive_new::new;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::{
    nodes::{
      st_ast::{ASTNodeData, StAst},
      type_name::TypeName,
    },
    types::Type,
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct SymbolNode {
  name: String,
}

impl SymbolNode {
  pub fn to_type_name_debug(self) -> TypeName {
    TypeName::new(vec![self.to_ast_debug()])
  }
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

  fn children(&self) -> Vec<&StAst> {
    vec![]
  }

  fn calc_type(&self, parent_type: Option<&Type>) -> (Option<String>, Type) {
    (Some(self.name().to_string()), parent_type.unwrap().clone())
  }
}
