use crate::{
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{nodes::ast_node::ASTNodeData, type_program::TypeProgram},
};
use serde_wasm_bindgen;
use wasm_bindgen::prelude::*;
pub mod semantic_types;

mod tests;

#[wasm_bindgen]
pub fn get_token_semantic_types() -> Vec<String> {
  SemanticType::all_variants()
    .iter()
    .map(|x| x.to_string().to_lowercase())
    .collect::<Vec<_>>()
}

#[wasm_bindgen]
pub fn analyze_program_semantics(src: String) -> JsValue {
  let result = analyze_program_semantics_internal(&src);
  let output = serde_wasm_bindgen::to_value(&result);
  match output {
    Ok(val) => val,
    Err(err) => panic!("{}", err),
  }
}

pub fn analyze_program_semantics_internal(src: &str) -> Vec<SemanticToken> {
  let tokens = TypeProgram::lex(&src);

  if let Err(_) = tokens {
    return vec![];
  }

  let mut semantic_tokens = tokens
    .as_ref()
    .unwrap()
    .iter()
    .map(|x| x.into_semantic_token())
    .collect::<Vec<_>>();

  let type_program = TypeProgram::parse(tokens);

  if let Ok(type_program) = type_program {
    println!("Program Parsed");
    type_program.update_semantics(&mut semantic_tokens);
  }

  semantic_tokens
    .iter()
    .filter(|x| x.token_type != SemanticType::None)
    .cloned()
    .collect::<Vec<_>>()
}
