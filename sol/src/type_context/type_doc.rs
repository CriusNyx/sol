use crate::type_program::{TypeProgram, lex_type_program, parse_type_program};
use derive_getters::Getters;
use wasm_bindgen::prelude::wasm_bindgen;

#[derive(Debug, Getters)]
#[wasm_bindgen]
pub struct TypeDoc {
  doc_identifier: String,
  doc_source: Option<String>,
  ast: Option<TypeProgram>,
}

impl TypeDoc {
  pub fn new(doc_identifier: String) -> TypeDoc {
    TypeDoc {
      doc_identifier,
      doc_source: None,
      ast: None,
    }
  }

  pub fn parse(&mut self, source: String) {
    self.ast = parse_type_program(&lex_type_program(&source)).ok();
    self.doc_source = Some(source);
  }
}
