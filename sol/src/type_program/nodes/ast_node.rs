use derive_getters::Getters;
use enum_dispatch::enum_dispatch;
use std::{cell::RefCell, iter::once, ops::Range};
use strum_macros::{EnumDiscriminants, EnumTryAs};

use crate::{
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{
    nodes::{
      array_decl::ArrayDecl, field_decl::FieldDecl, generic_param_decl::GenericParamDecl,
      global_decl::GlobalDecl, identifier::IdentifierDecl, lambda_decl::LambdaDecl,
      method_decl::MethodDecl, method_param_decl::MethodParamDecl, symbol_node::SymbolNode,
      type_decl::TypeDecl, type_program_node::TypeProgramNode, type_ref_decl::TypeRefDecl,
      unit_decl::UnitDecl,
    },
    program_equivalent::ProgramEquivalent,
    type_system::Type,
  },
};

#[derive(Getters, Debug)]
pub struct ASTNode {
  range: Range<usize>,
  data: NodeData,
  #[getter(skip)]
  cached_type: RefCell<Option<(Option<String>, Type)>>,
}

impl ASTNode {
  pub fn new(range: Range<usize>, data: NodeData) -> Self {
    ASTNode {
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

  pub fn traverse<Visitor: FnMut(&ASTNode)>(&self, visitor: &mut Visitor) {
    visitor(self);
    for child in self.children() {
      child.traverse(visitor);
    }
  }

  pub fn collect(&self) -> Vec<&ASTNode> {
    once(self)
      .chain(self.children().iter().map(|x| x.collect()).flatten())
      .collect::<Vec<_>>()
  }

  pub fn format_param_set(params: &Vec<ASTNode>) -> String {
    format_set("(", ")", ", ", params)
  }

  pub fn format_generic_param_set(params: &Vec<ASTNode>) -> String {
    format_set("<", ">", ", ", params)
  }

  pub fn format_body(params: &Vec<ASTNode>) -> String {
    format_set("{\n", "\n}", "\n", params)
  }

  pub fn format_inherits(params: &Vec<ASTNode>) -> String {
    format_set(": ", "", " + ", params)
  }

  pub fn update_semantics(ast_node: &ASTNode, tokens: &mut Vec<SemanticToken>) {
    println!("Update node");
    ASTNode::traverse(ast_node, &mut |x| x.update_semantics(tokens));
  }
}

impl Clone for ASTNode {
  fn clone(&self) -> Self {
    ASTNode::new(self.range.clone(), self.data.clone())
  }
}

impl ASTNodeData for ASTNode {
  fn format_source(&self) -> String {
    self.data.format_source()
  }

  fn children(&self) -> Vec<&ASTNode> {
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
  fn to_ast(self, range: Range<usize>) -> ASTNode;
  fn to_ast_debug(self) -> ASTNode;
  fn to_ast_boxed_debug(self) -> Box<ASTNode>;
}

impl<T: Into<NodeData>> ToAST for T {
  fn to_ast(self, range: Range<usize>) -> ASTNode {
    ASTNode::new(range, self.into())
  }

  fn to_ast_debug(self) -> ASTNode {
    ASTNode::new(0..0, self.into())
  }

  fn to_ast_boxed_debug(self) -> Box<ASTNode> {
    Box::new(self.to_ast_debug())
  }
}

pub trait CalcType {}

#[enum_dispatch]
pub trait ASTNodeData: ProgramEquivalent {
  fn format_source(&self) -> String;
  fn children(&self) -> Vec<&ASTNode>;
  fn calc_type(&self, _parent_type: Option<&Type>) -> (Option<String>, Type);
  fn update_semantics(&self, _tokens: &mut Vec<SemanticToken>) {}
}

pub fn format_set(start: &str, end: &str, separator: &str, params: &Vec<ASTNode>) -> String {
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
