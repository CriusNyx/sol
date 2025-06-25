use chumsky::container::Container;
use std::collections::HashMap;
use wasm_bindgen::prelude::wasm_bindgen;

use crate::type_context::type_doc::TypeDoc;

#[derive(Debug)]
#[wasm_bindgen]
pub struct TypeSystemContext {
  docs: HashMap<String, TypeDoc>,
}

impl TypeSystemContext {
  pub fn new() -> TypeSystemContext {
    TypeSystemContext {
      docs: HashMap::new(),
    }
  }

  pub fn new_doc(&mut self, doc_ident: &String) -> &TypeDoc {
    let output = TypeDoc::new(doc_ident.into());
    self.docs.push((doc_ident.into(), output));
    self.docs.get(doc_ident).unwrap()
  }

  pub fn get_doc_mut(&mut self, doc_ident: &String) -> Option<&mut TypeDoc> {
    self.docs.get_mut(doc_ident)
  }

  pub fn remove_doc(&mut self, doc_ident: &String) {
    self.docs.remove(doc_ident);
  }
}

#[wasm_bindgen]
pub fn create_type_system_context() -> TypeSystemContext {
  TypeSystemContext::new()
}
