use std::convert::identity;

use crate::type_program_old::{
  ClassBodyStatement, ClassDecl, FieldDef, GenericParamDecl, GlobalExp, GlobalVar, Identifier,
  LambdaDeclAST, MethodDecl, MethodParamDecl, TypeProgram, TypeRefAST,
};

pub trait TypeProgramVisitor {
  fn visit_program(&mut self, _program: &TypeProgram) {}
  fn visit_program_after(&mut self, _program: &TypeProgram) {}

  fn visit_global_exp(&mut self, _global_exp: &GlobalExp) {}
  fn visit_global_exp_after(&mut self, _global_exp: &GlobalExp) {}

  fn visit_class_decl(&mut self, _class_decl: &ClassDecl) {}
  fn visit_class_decl_after(&mut self, _class_decl: &ClassDecl) {}

  fn visit_global_var(&mut self, _global_var: &GlobalVar) {}
  fn visit_global_var_after(&mut self, _global_var: &GlobalVar) {}

  fn visit_generic_param_decl(&mut self, _generic_param_decl: &GenericParamDecl) {}
  fn visit_generic_param_decl_after(&mut self, _generic_param_decl: &GenericParamDecl) {}

  fn visit_type_ref(&mut self, _type_ref: &TypeRefAST) {}
  fn visit_type_ref_after(&mut self, _type_decl: &TypeRefAST) {}

  fn visit_lambda(&mut self, _lambda: &LambdaDeclAST) {}
  fn visit_lambda_after(&mut self, _lambda: &LambdaDeclAST) {}

  fn visit_class_statement(&mut self, _class_statement: &ClassBodyStatement) {}
  fn visit_class_statement_after(&mut self, _class_statement: &ClassBodyStatement) {}

  fn visit_field_decl(&mut self, _field_decl: &FieldDef) {}
  fn visit_field_decl_after(&mut self, _field_decl: &FieldDef) {}

  fn visit_identifier(&mut self, _identifier: &Identifier) {}
  fn visit_identifier_after(&mut self, _identifier: &Identifier) {}

  fn visit_method_decl(&mut self, _method_decl: &MethodDecl) {}
  fn visit_method_decl_after(&mut self, _method_decl: &MethodDecl) {}

  fn visit_method_param(&mut self, _method_decl: &MethodParamDecl) {}
  fn visit_method_param_after(&mut self, _method_decl: &MethodParamDecl) {}
}

fn visit_type_program(program: &TypeProgram, visitor: &mut impl TypeProgramVisitor) {
  fn visit_type_ref(type_ref: &TypeRefAST, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_type_ref(type_ref);

    match type_ref {
      TypeRefAST::ArrayTypeRef(arr) => visit_type_ref(&arr.array_type, visitor),
      TypeRefAST::SymTypeRef(sym) => {
        sym.params.as_ref().map(|param| {
          for type_ref_param in param {
            visit_type_ref(type_ref_param, visitor);
          }
        });
      }
      TypeRefAST::LambdaDecl(lambda) => {
        visit_lambda(lambda, visitor);
      }
    };

    visitor.visit_type_ref_after(type_ref);
  }

  fn visit_lambda(lambda_decl: &LambdaDeclAST, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_lambda(lambda_decl);
    for generic_param in lambda_decl.generic_params.iter().flat_map(identity) {
      visit_generic_param_decl(generic_param, visitor);
    }

    for param in &lambda_decl.param_types {
      visit_method_param(param, visitor);
    }

    if let Some(ret) = &lambda_decl.return_type {
      visit_type_ref(ret, visitor);
    }

    visitor.visit_lambda_after(lambda_decl);
  }

  fn visit_generic_param_decl(
    generic_param_decl: &GenericParamDecl,
    visitor: &mut impl TypeProgramVisitor,
  ) {
    visitor.visit_generic_param_decl(generic_param_decl);

    generic_param_decl.inherits.as_ref().map(|inherits| {
      for type_ref in inherits {
        visit_type_ref(type_ref, visitor);
      }
    });

    visitor.visit_generic_param_decl_after(generic_param_decl);
  }

  fn visit_identifier(identifier: &Identifier, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_identifier(identifier);

    visit_type_ref(&identifier.type_decl, visitor);

    visitor.visit_identifier_after(identifier);
  }

  fn visit_field_decl(field_decl: &FieldDef, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_field_decl(field_decl);

    visit_identifier(&field_decl.identifier, visitor);

    visitor.visit_field_decl_after(field_decl);
  }

  fn visit_method_param(param: &MethodParamDecl, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_method_param(param);

    visit_type_ref(&param.type_ref, visitor);

    visitor.visit_method_param_after(param);
  }

  fn visit_method_decl(method_decl: &MethodDecl, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_method_decl(method_decl);

    method_decl.generic_params.as_ref().map(|generic_params| {
      for param in generic_params {
        visit_generic_param_decl(param, visitor);
      }
    });

    method_decl
      .return_type
      .as_ref()
      .map(|return_type| visit_type_ref(return_type, visitor));

    for param in &method_decl.param_types {
      visit_method_param(param, visitor);
    }

    visitor.visit_method_decl_after(method_decl);
  }

  fn visit_class_statement(
    class_statement: &ClassBodyStatement,
    visitor: &mut impl TypeProgramVisitor,
  ) {
    visitor.visit_class_statement(class_statement);

    match class_statement {
      ClassBodyStatement::FieldDecl(field_decl) => visit_field_decl(field_decl, visitor),
      ClassBodyStatement::MethodDecl(method_decl) => visit_method_decl(method_decl, visitor),
    }

    visitor.visit_class_statement_after(class_statement);
  }

  fn visit_class_decl(class_decl: &ClassDecl, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_class_decl(class_decl);

    class_decl.generic_params.as_ref().map(|generic_params| {
      for generic_param in generic_params {
        visit_generic_param_decl(generic_param, visitor);
      }
    });

    class_decl.inherits.as_ref().map(|inherits| {
      for param in inherits {
        visit_type_ref(param, visitor);
      }
    });

    class_decl.body.as_ref().map(|body| {
      for class_statement in body {
        visit_class_statement(class_statement, visitor);
      }
    });

    visitor.visit_class_decl_after(class_decl);
  }

  fn visit_global_var(global_var: &GlobalVar, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_global_var(global_var);

    visit_identifier(&global_var.identifier, visitor);

    visitor.visit_global_var_after(global_var);
  }

  fn visit_global_exp(global_exp: &GlobalExp, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_global_exp(global_exp);

    match global_exp {
      GlobalExp::ClassDec(class_decl) => visit_class_decl(class_decl, visitor),
      GlobalExp::GlobalVar(global_var) => visit_global_var(global_var, visitor),
    }

    visitor.visit_global_exp_after(global_exp);
  }

  fn visit_program(program: &TypeProgram, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_program(&program);

    for global_exp in &program.expressions {
      visit_global_exp(global_exp, visitor);
    }

    visitor.visit_program_after(program);
  }

  visit_program(&program, visitor);
}

impl TypeProgram {
  pub fn visit(&self, visitor: &mut impl TypeProgramVisitor) {
    visit_type_program(self, visitor);
  }
}
