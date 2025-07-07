use derive_getters::Getters;
use derive_new::new;

use crate::type_program::{
  nodes::ast_node::{ASTNode, ASTNodeData},
  program_equivalent::ProgramEquivalent,
  type_system::Type,
};

#[derive(new, Getters, Debug, Clone)]
pub struct LambdaDecl {
  generic_params: Option<Vec<ASTNode>>,
  params: Vec<ASTNode>,
  return_type: Option<Box<ASTNode>>,
}

impl ProgramEquivalent for LambdaDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.params().program_equivalent(b.params())
      && self.return_type().program_equivalent(b.return_type())
  }
}

impl ASTNodeData for LambdaDecl {
  fn format_source(&self) -> String {
    format!(
      "{}{} => {}",
      self
        .generic_params()
        .as_ref()
        .map(ASTNode::format_generic_param_set)
        .unwrap_or("".to_string()),
      ASTNode::format_param_set(self.params()),
      self
        .return_type()
        .as_ref()
        .map(|x| x.format_source())
        .unwrap_or("void".to_string())
    )
  }

  fn children(&self) -> Vec<&ASTNode> {
    self
      .generic_params()
      .iter()
      .flatten()
      .chain(self.params().iter())
      .chain(self.return_type().iter().map(|x| x.as_ref()))
      .collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    (
      None,
      Type::from_method(&self.params, &self.generic_params, &self.return_type),
    )
  }
}
