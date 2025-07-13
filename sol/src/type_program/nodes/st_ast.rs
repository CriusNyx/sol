use derive_getters::Getters;
use enum_dispatch::enum_dispatch;
use std::{cell::RefCell, iter::once, ops::Range};
use strum_macros::{EnumDiscriminants, EnumTryAs};

use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{
    nodes::{
      array_decl::ArrayDecl, field_decl::FieldDecl, generic_param_decl::GenericParamDecl,
      global_decl::GlobalDecl, identifier::IdentifierDecl, lambda_decl::LambdaDecl,
      method_decl::MethodDecl, method_param_decl::MethodParamDecl, symbol_node::SymbolNode,
      type_decl::TypeDecl, type_program_node::TypeProgramNode, type_ref_decl::TypeRefDecl,
      unit_decl::UnitDecl,
    },
    types::Type,
  },
};

#[derive(Getters, Debug)]
pub struct StAst {
  range: Range<usize>,
  data: NodeData,
  #[getter(skip)]
  cached_type: RefCell<Option<(Option<String>, Type)>>,
}

impl StAst {
  pub fn new(range: Range<usize>, data: NodeData) -> Self {
    StAst {
      range,
      data,
      cached_type: None.into(),
    }
  }

  pub fn get_type(&self) -> (Option<String>, Type) {
    self.cached_type.borrow().as_ref().unwrap().clone()
  }

  pub fn sym_name(&self) -> Option<String> {
    self
      .data()
      .try_as_symbol_node_ref()
      .map(|x| x.name().to_string())
  }

  pub fn apply_semantics(&self, tokens: &mut Vec<SemanticToken>, new_token: &SemanticType) {
    for element in self.range().clone() {
      tokens[element].token_type = new_token.clone();
    }
  }

  pub fn traverse<Visitor: FnMut(&StAst)>(&self, visitor: &mut Visitor) {
    visitor(self);
    for child in self.children() {
      child.traverse(visitor);
    }
  }

  pub fn collect(&self) -> Vec<&StAst> {
    once(self)
      .chain(self.children().iter().map(|x| x.collect()).flatten())
      .collect::<Vec<_>>()
  }

  pub fn format_param_set(params: &Vec<StAst>) -> String {
    format_set("(", ")", ", ", params)
  }

  pub fn format_generic_param_set(params: &Vec<StAst>) -> String {
    format_set("<", ">", ", ", params)
  }

  pub fn format_body(params: &Vec<StAst>) -> String {
    format_set("{\n", "\n}", "\n", params)
  }

  pub fn format_inherits(params: &Vec<StAst>) -> String {
    format_set(": ", "", " + ", params)
  }

  pub fn update_semantics(ast_node: &StAst, tokens: &mut Vec<SemanticToken>) {
    println!("Update node");
    StAst::traverse(ast_node, &mut |x| x.update_semantics(tokens));
  }
}

impl Clone for StAst {
  fn clone(&self) -> Self {
    StAst::new(self.range.clone(), self.data.clone())
  }
}

impl ASTNodeData for StAst {
  fn format_source(&self) -> String {
    self.data.format_source()
  }

  fn children(&self) -> Vec<&StAst> {
    self.data.children()
  }

  fn calc_type(&self, parent_type: Option<&Type>) -> (Option<String>, Type) {
    self
      .cached_type
      .borrow_mut()
      .get_or_insert_with(|| self.data().calc_type(parent_type))
      .clone()
  }

  fn update_semantics(&self, tokens: &mut Vec<SemanticToken>) {
    self.data().update_semantics(tokens);
  }
}

#[derive(Debug, Clone, EnumDiscriminants, EnumTryAs)]
#[enum_dispatch(ASTNodeData)]
pub enum NodeData {
  SymbolNode(SymbolNode),
  ArrayDecl(ArrayDecl),
  TypeRefDecl(TypeRefDecl),
  LambdaDecl(LambdaDecl),
  MethodParamDecl(MethodParamDecl),
  GenericParamDecl(GenericParamDecl),
  IdentifierDecl(IdentifierDecl),
  TypeDecl(TypeDecl),
  FieldDecl(FieldDecl),
  MethodDecl(MethodDecl),
  GlobalDecl(GlobalDecl),
  TypeProgramNode(TypeProgramNode),
  UnitDecl(UnitDecl),
}

pub trait ToAST {
  fn to_ast(self, range: Range<usize>) -> StAst;
  fn to_ast_debug(self) -> StAst;
  fn to_ast_boxed_debug(self) -> Box<StAst>;
}

impl<T: Into<NodeData>> ToAST for T {
  fn to_ast(self, range: Range<usize>) -> StAst {
    StAst::new(range, self.into())
  }

  fn to_ast_debug(self) -> StAst {
    StAst::new(0..0, self.into())
  }

  fn to_ast_boxed_debug(self) -> Box<StAst> {
    Box::new(self.to_ast_debug())
  }
}

pub trait CalcType {}

#[enum_dispatch]
pub trait ASTNodeData: ProgramEquivalent {
  fn format_source(&self) -> String;
  fn children(&self) -> Vec<&StAst>;
  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type);
  fn update_semantics(&self, _tokens: &mut Vec<SemanticToken>) {}
}

pub fn format_set(start: &str, end: &str, separator: &str, params: &Vec<StAst>) -> String {
  format!(
    "{}{}{}",
    start,
    params
      .iter()
      .map(|x| x.format_source())
      .collect::<Vec<_>>()
      .join(&separator),
    end
  )
}
