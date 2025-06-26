use chumsky::container::Container;
use std::collections::HashMap;
use wasm_bindgen::prelude::wasm_bindgen;

use crate::type_context::type_doc::TypeDoc;

#[derive(Debug)]
#[wasm_bindgen]
pub struct TypeSystemContext {
  docs: HashMap<String, TypeDoc>,
}

#[wasm_bindgen]
impl TypeSystemContext {
  pub fn new() -> TypeSystemContext {
    TypeSystemContext {
      docs: HashMap::new().into(),
    }
  }

  #[wasm_bindgen]
  pub fn new_doc(&mut self, doc_ident: String) {
    let output = TypeDoc::new(doc_ident.to_string());
    self.docs.push((doc_ident.to_string(), output.into()));
  }

  #[wasm_bindgen]
  pub fn remove_doc(&mut self, doc_ident: String) {
    self.docs.remove(&doc_ident);
  }

  #[wasm_bindgen]
  pub fn update_doc_text(&mut self, doc_ident: String, source: String) {
    self.docs.get_mut(&doc_ident).unwrap().set_source(source);
  }

  pub fn get_doc_identifiers(self) -> Vec<String> {
    self
      .docs
      .keys()
      .into_iter()
      .map(|x| x.clone())
      .collect::<Vec<_>>()
  }
}

#[wasm_bindgen]
pub fn create_type_system_context() -> TypeSystemContext {
  TypeSystemContext::new()
}
