// use derive_new::new;
// use std::{cell::RefCell, rc::Rc};
// use wasm_bindgen::prelude::wasm_bindgen;

// use crate::type_context::type_doc::TypeDoc;

// #[derive(new, Debug)]
// #[wasm_bindgen]
// pub struct TypeDocRef {
//   doc: Rc<RefCell<TypeDoc>>,
// }

// #[wasm_bindgen]
// impl TypeDocRef {
//   pub fn set_source(&self, source: String) {
//     self.doc.borrow_mut().set_source(source);
//   }
// }
