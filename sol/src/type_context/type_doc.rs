use derive_getters::Getters;
use wasm_bindgen::prelude::wasm_bindgen;

use crate::{
  lsp::{semantic_analysis::update_semantic_token_info, semantic_types::SemanticToken},
  type_program::{TypeProgram, TypeToken, lex_type_program, parse_type_program},
};

#[derive(Debug, Getters)]
#[wasm_bindgen]
pub struct TypeDoc {
  doc_identifier: String,
  doc_source: Option<String>,
  tokens: Option<Vec<TypeToken>>,
  semantics: Option<Vec<SemanticToken>>,
  ast: Option<TypeProgram>,
}

impl TypeDoc {
  pub fn new(doc_identifier: String) -> TypeDoc {
    TypeDoc {
      doc_identifier,
      doc_source: None,
      tokens: None,
      semantics: None,
      ast: None,
    }
  }

  pub fn set_source(&mut self, source: String) {
    let token_result = lex_type_program(&source);
    self.tokens = token_result.as_ref().ok().cloned();
    self.semantics = self.tokens.as_ref().map(|x| {
      x.into_iter()
        .map(|y| y.into_semantic_token())
        .collect::<Vec<_>>()
    });

    let parse_result = parse_type_program(&token_result);

    match (&parse_result, self.semantics.as_mut()) {
      (Ok(result), Some(semantics)) => {
        update_semantic_token_info(&result, semantics);
      }
      _ => (),
    };

    self.ast = parse_result.ok();
    self.doc_source = Some(source).into();
  }
}
