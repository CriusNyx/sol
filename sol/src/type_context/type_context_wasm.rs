use wasm_bindgen::prelude::wasm_bindgen;

use crate::type_context::type_context::TypeSystemContext;

#[wasm_bindgen]
pub fn create_type_context() -> TypeSystemContext {
  TypeSystemContext::new()
}
