// use serde::Serialize;
// use std::{cell::RefCell, collections::HashMap, rc::Rc};
// use wasm_bindgen::{JsValue, prelude::wasm_bindgen};

// use crate::type_context::{type_doc::TypeDoc, type_doc_ref::TypeDocRef};

// #[derive(Debug, Serialize)]
// enum TypeContextError {
//   NoDocWithIdentifier,
// }

// #[derive(Debug)]
// #[wasm_bindgen]
// pub struct TypeSystemContext {
//   docs: HashMap<String, Rc<RefCell<TypeDoc>>>,
// }

// #[wasm_bindgen]
// impl TypeSystemContext {
//   pub fn new() -> TypeSystemContext {
//     TypeSystemContext {
//       docs: HashMap::new(),
//     }
//   }

//   #[wasm_bindgen]
//   pub fn new_doc(&mut self, doc_ident: String) {
//     let output = TypeDoc::new(doc_ident.to_string());
//     self
//       .docs
//       .insert(doc_ident.to_string(), Rc::new(RefCell::new(output)));
//   }

//   #[wasm_bindgen]
//   pub fn remove_doc(&mut self, doc_ident: String) {
//     self.docs.remove(&doc_ident);
//   }

//   #[wasm_bindgen]
//   pub fn borrow(&self, doc_ident: String) -> Result<TypeDocRef, JsValue> {
//     self
//       .docs
//       .get(&doc_ident)
//       .map(|doc| TypeDocRef::new(doc.clone()))
//       .ok_or(serde_wasm_bindgen::to_value(&TypeContextError::NoDocWithIdentifier).unwrap())
//   }

//   #[wasm_bindgen]
//   pub fn get_doc_identifiers(&self) -> Vec<String> {
//     self
//       .docs
//       .keys()
//       .into_iter()
//       .map(|x| x.clone())
//       .collect::<Vec<_>>()
//   }
// }
