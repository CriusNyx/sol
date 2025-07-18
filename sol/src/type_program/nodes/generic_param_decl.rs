use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{
    nodes::st_ast::{ASTNodeData, StAst},
    types::{GenericType, Type},
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct GenericParamDecl {
  name: Box<StAst>,
  inherits: Option<Vec<StAst>>,
}

impl ProgramEquivalent for GenericParamDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.name().program_equivalent(b.name()) && self.inherits().program_equivalent(b.inherits())
  }
}

impl ASTNodeData for GenericParamDecl {
  fn format_source(&self) -> String {
    format!(
      "{}{}",
      self.name().format_source(),
      self
        .inherits()
        .as_ref()
        .map(|x| ": ".to_owned()
          + &x
            .iter()
            .map(|y| y.format_source())
            .collect::<Vec<_>>()
            .join(" + "))
        .unwrap_or("".to_string())
    )
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.name().as_ref())
      .chain(self.inherits().iter().flatten())
      .collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let output: Type = GenericType::new(self.name().type_name().unwrap()).into();

    self.name().calc_type(Some(&output));

    (self.name().type_name(), output)
  }

  fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    self.name().apply_semantics(tokens, &SemanticType::Type);
  }
}
