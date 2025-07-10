use logos::Logos;
use wasm_bindgen::prelude::*;

use crate::type_program::type_token::TypeToken;

#[wasm_bindgen]
pub fn compile_types(source: &str) -> JsValue {
  let tokens: Vec<TypeToken> = TypeToken::lexer(source).collect::<Result<_, _>>().unwrap();

  todo!();

  // let ast = type_parser().parse(&tokens).into_result();

  // serde_wasm_bindgen::to_value(&ast.unwrap()).unwrap()
}
