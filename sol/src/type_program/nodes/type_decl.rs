use derive_getters::Getters;
use derive_new::new;
use std::{collections::HashMap, iter::once, rc::Rc};

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{
    nodes::ast_node::{ASTNode, ASTNodeData},
    types::{ObjectType, Type, TypeImpl},
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct TypeDecl {
  name: Box<ASTNode>,
  generic_params: Option<Vec<ASTNode>>,
  inherits: Option<Vec<ASTNode>>,
  body: Option<Vec<ASTNode>>,
}

impl ProgramEquivalent for TypeDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.name().program_equivalent(b.name())
      && self.inherits().program_equivalent(b.inherits())
      && self.body().program_equivalent(b.body())
  }
}

impl ASTNodeData for TypeDecl {
  fn format_source(&self) -> String {
    format!(
      "type {}{}{}{}",
      self.name().format_source(),
      self
        .generic_params()
        .as_ref()
        .map(|x| ASTNode::format_generic_param_set(x))
        .unwrap_or("".to_string()),
      self
        .inherits()
        .as_ref()
        .map(ASTNode::format_inherits)
        .unwrap_or("".to_string()),
      self
        .body()
        .as_ref()
        .map(|x| " ".to_owned() + &ASTNode::format_body(x))
        .unwrap_or(";".to_string())
    )
  }

  fn children(&self) -> Vec<&ASTNode> {
    once(self.name().as_ref())
      .chain(self.generic_params().iter().flatten())
      .chain(self.inherits().iter().flatten())
      .chain(self.body().iter().flatten())
      .collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let name = self.name().sym_name().unwrap();
    let generic_params = self.generic_params().as_ref().map(|x| {
      x.iter()
        .map(|y| y.calc_type(None).1.to_rc())
        .collect::<Vec<_>>()
    });
    let inherits = self.inherits().as_ref().map(|x| {
      x.iter()
        .map(|y| y.calc_type(None).1.to_rc())
        .collect::<Vec<_>>()
    });

    let mut body = HashMap::<String, Rc<Type>>::new();

    for statement in self.body().iter().flatten() {
      let (name, t) = statement.calc_type(None);
      body.insert(name.unwrap(), t.to_rc());
    }

    let output: Type = ObjectType::new(name.to_string(), inherits, generic_params, body).into();

    self.name().calc_type(Some(&output));

    (Some(name.to_string()), output)
  }

  fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    self.name().apply_semantics(tokens, &SemanticType::Type);
  }
}
