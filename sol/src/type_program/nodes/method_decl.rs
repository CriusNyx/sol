use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{
    nodes::st_ast::{ASTNodeData, StAst},
    types::Type,
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct MethodDecl {
  name: Box<StAst>,
  generic_params: Option<Vec<StAst>>,
  params: Vec<StAst>,
  return_type: Option<Box<StAst>>,
  is_static: bool,
}

impl ProgramEquivalent for MethodDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.name().program_equivalent(b.name())
      && self.generic_params().program_equivalent(b.generic_params())
      && self.params().program_equivalent(b.params())
      && self.return_type().program_equivalent(b.return_type())
      && *self.is_static() == *b.is_static()
  }
}

impl ASTNodeData for MethodDecl {
  fn format_source(&self) -> String {
    format!(
      "{}{}{}{}{};",
      if *self.is_static() { "static " } else { "" },
      self.name().format_source(),
      self
        .generic_params()
        .as_ref()
        .map(StAst::format_generic_param_set)
        .unwrap_or("".to_string()),
      StAst::format_param_set(self.params()),
      self
        .return_type()
        .as_ref()
        .map(|x| ": ".to_owned() + &x.format_source())
        .unwrap_or("".to_string())
    )
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.name().as_ref())
      .chain(self.generic_params().iter().flatten())
      .chain(self.params().iter())
      .chain(self.return_type().iter().map(|x| x.as_ref()))
      .collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let output = Type::from_method_overload(&self.params, &self.generic_params, &self.return_type);

    // Set name type
    self.name().calc_type(Some(&output));

    (Some(self.name().type_name().unwrap()), output)
  }

  fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    self.name().apply_semantics(tokens, &SemanticType::Method);
  }
}
