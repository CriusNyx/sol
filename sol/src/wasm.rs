use logos::Logos;
use wasm_bindgen::prelude::*;

use crate::type_program::st_token::StToken;

#[wasm_bindgen]
pub fn compile_types(source: &str) -> JsValue {
  let tokens: Vec<StToken> = StToken::lexer(source).collect::<Result<_, _>>().unwrap();

  todo!();

  // let ast = type_parser().parse(&tokens).into_result();

  // serde_wasm_bindgen::to_value(&ast.unwrap()).unwrap()
}
