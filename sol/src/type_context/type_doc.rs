use crate::type_program::{TypeProgram, lex_type_program, parse_type_program};
use derive_getters::Getters;
use std::sync::Arc;
use wasm_bindgen::prelude::wasm_bindgen;

#[derive(Debug, Getters)]
#[wasm_bindgen]
pub struct TypeDoc {
  doc_identifier: String,
  doc_source: Arc<Option<String>>,
  ast: Option<TypeProgram>,
}

impl TypeDoc {
  pub fn new(doc_identifier: String) -> TypeDoc {
    TypeDoc {
      doc_identifier,
      doc_source: None.into(),
      ast: None.into(),
    }
  }

  pub fn set_source(&mut self, source: String) {
    self.ast = parse_type_program(&lex_type_program(&source)).ok();
    self.doc_source = Some(source).into();
  }
}
