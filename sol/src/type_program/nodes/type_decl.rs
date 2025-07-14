use derive_getters::Getters;
use derive_new::new;
use std::{cell::RefCell, collections::HashMap, iter::once};

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{
    nodes::st_ast::{ASTNodeData, StAst},
    types::{MethodType, ObjectType, Type, TypeImpl},
  },
};

#[derive(new, Getters, Debug, Clone)]
pub struct TypeDecl {
  name: Box<StAst>,
  generic_params: Option<Vec<StAst>>,
  inherits: Option<Vec<StAst>>,
  body: Option<Vec<StAst>>,
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
        .map(|x| StAst::format_generic_param_set(x))
        .unwrap_or("".to_string()),
      self
        .inherits()
        .as_ref()
        .map(StAst::format_inherits)
        .unwrap_or("".to_string()),
      self
        .body()
        .as_ref()
        .map(|x| " ".to_owned() + &StAst::format_body(x))
        .unwrap_or(";".to_string())
    )
  }

  fn children(&self) -> Vec<&StAst> {
    once(self.name().as_ref())
      .chain(self.generic_params().iter().flatten())
      .chain(self.inherits().iter().flatten())
      .chain(self.body().iter().flatten())
      .collect()
  }

  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type) {
    let name = self.name().type_name().unwrap();
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

    let mut body = HashMap::<String, RefCell<Type>>::new();

    for statement in self.body().iter().flatten() {
      let (name, statement_type) = statement.calc_type(None);
      let name = name.unwrap();
      match statement_type {
        Type::MethodType(method_type) => {
          let element = body
            .entry(name.to_string())
            .or_insert_with(|| RefCell::new(MethodType::new(vec![]).into()));

          for overload in method_type.overloads() {
            element
              .borrow_mut()
              .try_as_method_type_mut()
              .unwrap()
              .overloads_mut()
              .push(overload.clone());
          }
        }
        _ => {
          body.insert(name, RefCell::new(statement_type));
        }
      }
    }

    let output: Type = ObjectType::new(
      name.to_string(),
      inherits,
      generic_params,
      body
        .iter()
        .map(|(key, value)| (key.to_string(), value.clone().into_inner().to_rc()))
        .collect::<HashMap<_, _>>(),
    )
    .into();

    self.name().calc_type(Some(&output));

    (Some(name.to_string()), output)
  }

  fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    self.name().apply_semantics(tokens, &SemanticType::Type);
  }
}
