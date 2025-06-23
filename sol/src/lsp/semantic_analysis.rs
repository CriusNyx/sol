use crate::{
  lsp::semantic_types::{SemanticToken, SemanticType},
  type_program::{GenericParamDecl, TypeProgram, TypeProgramVisitor, TypeRef, TypeToken},
};

struct SemanticAnalysisVisitor<'visitor> {
  pub semantic_tokens: &'visitor mut Vec<SemanticToken>,
}

impl<'visitor> SemanticAnalysisVisitor<'visitor> {
  fn set_token_type(&mut self, type_token: &TypeToken, semantic_type: SemanticType) {
    self
      .semantic_tokens
      .get_mut(type_token.get_info().index as usize)
      .map(|semantic_token| {
        semantic_token.token_type = semantic_type;
      });
  }
}

impl<'visitor> TypeProgramVisitor for SemanticAnalysisVisitor<'visitor> {
  fn visit_generic_param_decl<'a>(&mut self, generic_param_decl: &GenericParamDecl<'a>) {
    self.set_token_type(&generic_param_decl.name, SemanticType::Type);
  }

  fn visit_method_decl<'a>(&mut self, method_decl: &crate::type_program::MethodDecl<'a>) {
    self.set_token_type(&method_decl.name, SemanticType::Method);
  }

  fn visit_class_decl<'a>(&mut self, class_decl: &crate::type_program::ClassDecl<'a>) {
    self.set_token_type(&class_decl.name, SemanticType::Type);
  }

  fn visit_type_ref<'a>(&mut self, type_ref: &TypeRef<'a>) {
    match type_ref {
      TypeRef::SymTypeRef(sym) => {
        self.set_token_type(&sym.name, SemanticType::Type);
      }
      _ => (),
    }
  }
}

/// Traverse the abstract syntax tree and update semantic info for each AST node.
pub fn update_semantic_token_info<'a>(
  program: TypeProgram<'a>,
  semantic_tokens: &mut Vec<SemanticToken>,
) {
  let mut visitor = SemanticAnalysisVisitor { semantic_tokens };
  program.visit(&mut visitor);
}
