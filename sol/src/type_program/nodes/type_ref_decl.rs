use derive_getters::Getters;
use derive_new::new;
use std::iter::once;

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{
    nodes::st_ast::{StAst, ASTNodeData},
    types::{RefType, Type, TypeImpl},
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct TypeRefDecl {
  name: Box<StAst>,
  generic_decl: Option<Vec<StAst>>,
}

impl ProgramEquivalent for TypeRefDecl {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.name().program_equivalent(b.name())
      && self.generic_decl().program_equivalent(b.generic_decl())
  }
}

impl ASTNodeData for TypeRefDecl {
  fn format_source(&self) -> String {
    format!(
      "{}{}",
      self.name().format_source(),
      self
        .generic_decl()
        .as_ref()
        .map(StAst::format_generic_param_set)
        .unwrap_or("".to_string())
    )
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.name().as_ref())
      .chain(self.generic_decl().iter().flatten())
      .collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let name: String = self.name().sym_name().unwrap();

    let generic_params = self.generic_decl().as_ref().map(|x| {
      x.iter()
        .map(|y| y.calc_type(None).1.to_rc())
        .collect::<Vec<_>>()
    });

    let output_type: Type = RefType::new(name.to_string(), generic_params).into();

    self.name().calc_type(Some(&output_type));

    (None, output_type.into())
  }

  fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    self.name().apply_semantics(tokens, &SemanticType::Type);
  }
}
