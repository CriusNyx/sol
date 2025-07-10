use std::collections::HashMap;

use derive_getters::Getters;
use derive_new::new;

use crate::type_program::{
  nodes::ast_node::{ASTNode, ASTNodeData},
  program_equivalent::ProgramEquivalent,
  types::{ProgramType, Type},
};

#[derive(new, Getters, Debug, Clone)]
pub struct TypeProgramNode {
  statements: Vec<ASTNode>,
}

impl ProgramEquivalent for TypeProgramNode {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.statements().program_equivalent(b.statements())
  }
}

impl ASTNodeData for TypeProgramNode {
  fn format_source(&self) -> String {
    self
      .statements()
      .iter()
      .map(|x| x.format_source())
      .collect::<Vec<_>>()
      .join("\n\n")
  }

  fn children(&self) -> Vec<&ASTNode> {
    self.statements().iter().collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let mut hash_map = HashMap::<String, Box<Type>>::new();

    for statement in self.statements().iter() {
      let result = statement.calc_type(None);
      hash_map.insert(result.0.unwrap().to_string(), Box::new(result.1));
    }

    (None, ProgramType::new(hash_map).into())
  }
}
