use crate::{
  helpers::program_equivalent::ProgramEquivalent,
  type_program::nodes::st_ast::{NodeData, StAst},
};

impl ProgramEquivalent for StAst {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.data().program_equivalent(b.data())
  }
}

impl ProgramEquivalent for NodeData {
  fn program_equivalent(&self, b: &Self) -> bool {
    match (self, b) {
      (Self::SymbolNode(a), Self::SymbolNode(b)) => a.program_equivalent(b),
      (Self::TypeName(a), Self::TypeName(b)) => a.program_equivalent(b),
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
