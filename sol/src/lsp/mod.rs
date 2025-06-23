use crate::{
  lsp::{
    semantic_analysis::update_semantic_token_info,
    semantic_types::{SemanticToken, SemanticType},
  },
  type_program::{lex_type_program, parse_type_program},
};
use serde_wasm_bindgen;
use wasm_bindgen::prelude::*;
pub mod semantic_analysis;
pub mod semantic_types;

#[wasm_bindgen]
pub fn get_token_semantic_types() -> Vec<String> {
  SemanticType::all_variants()
    .iter()
    .map(|x| x.to_string().to_lowercase())
    .collect::<Vec<_>>()
}

#[wasm_bindgen]
pub fn analyze_program_semantics(src: String) -> JsValue {
  let result = analyze_program_semantics_internal(src);
  let output = serde_wasm_bindgen::to_value(&result);
  match output {
    Ok(val) => val,
    Err(err) => panic!("{}", err),
  }
}

pub fn analyze_program_semantics_internal(src: String) -> Vec<SemanticToken> {
  let lex_result = lex_type_program(&src);

  let ast_result = parse_type_program(&lex_result);

  match ast_result {
    Ok(ast) => {
      let mut semantic_tokens = lex_result
        .as_ref()
        .unwrap()
        .iter()
        .map(|x| x.into_semantic_token())
        .collect::<Vec<_>>();

      update_semantic_token_info(ast, &mut semantic_tokens);

      semantic_tokens
    }
    Err(_) => lex_result
      .as_ref()
      .unwrap()
      .iter()
      .map(|x| x.into_semantic_token())
      .collect::<Vec<_>>(),
  }
}
