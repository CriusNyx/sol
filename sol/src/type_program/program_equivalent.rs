use crate::type_program::nodes::ast_node::{ASTNode, NodeData};

pub trait ProgramEquivalent {
  fn program_equivalent(&self, b: &Self) -> bool;
}

impl ProgramEquivalent for ASTNode {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.data().program_equivalent(b.data())
  }
}

impl ProgramEquivalent for NodeData {
  fn program_equivalent(&self, b: &Self) -> bool {
    match (self, b) {
      (Self::SymbolNode(a), Self::SymbolNode(b)) => a.program_equivalent(b),
      (Self::ArrayDecl(a), Self::ArrayDecl(b)) => a.program_equivalent(b),
      (Self::TypeRefDecl(a), Self::TypeRefDecl(b)) => a.program_equivalent(b),
      (Self::LambdaDecl(a), Self::LambdaDecl(b)) => a.program_equivalent(b),
      (Self::MethodParamDecl(a), Self::MethodParamDecl(b)) => a.program_equivalent(b),
      (Self::GenericParamDecl(a), Self::GenericParamDecl(b)) => a.program_equivalent(b),
      (Self::IdentifierDecl(a), Self::IdentifierDecl(b)) => a.program_equivalent(b),
      (Self::TypeDecl(a), Self::TypeDecl(b)) => a.program_equivalent(b),
      (Self::FieldDecl(a), Self::FieldDecl(b)) => a.program_equivalent(b),
      (Self::MethodDecl(a), Self::MethodDecl(b)) => a.program_equivalent(b),
      (Self::GlobalDecl(a), Self::GlobalDecl(b)) => a.program_equivalent(b),
      (Self::TypeProgramNode(a), Self::TypeProgramNode(b)) => a.program_equivalent(b),
      (Self::UnitDecl(a), Self::UnitDecl(b)) => a.program_equivalent(b),
      _ => false,
    }
  }
}

impl<T: ProgramEquivalent> ProgramEquivalent for Box<T> {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.as_ref().program_equivalent(b.as_ref())
  }
}

impl<T: ProgramEquivalent> ProgramEquivalent for Option<T> {
  fn program_equivalent(&self, b: &Self) -> bool {
    match (self, b) {
      (Some(a), Some(b)) => a.program_equivalent(b),
      (None, None) => true,
      _ => false,
    }
  }
}

impl<T: ProgramEquivalent> ProgramEquivalent for Vec<T> {
  fn program_equivalent(&self, b: &Self) -> bool {
    self
      .iter()
      .zip(b.iter())
      .all(|(a, b)| a.program_equivalent(b))
  }
}
