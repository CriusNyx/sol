use crate::type_program::{
  ClassBodyStatement, ClassDecl, FieldDef, GenericParamDecl, MethodDecl, MethodParam, TypeProgram,
  TypeRef,
};

pub trait TypeProgramVisitor {
  fn visit_program<'a>(&mut self, _program: &TypeProgram<'a>) {}
  fn visit_program_after<'a>(&mut self, _program: &TypeProgram<'a>) {}
  fn visit_class_decl<'a>(&mut self, _class_decl: &ClassDecl<'a>) {}
  fn visit_class_decl_after<'a>(&mut self, _class_decl: &ClassDecl<'a>) {}
  fn visit_generic_param_decl<'a>(&mut self, _generic_param_decl: &GenericParamDecl<'a>) {}
  fn visit_generic_param_decl_after<'a>(&mut self, _generic_param_decl: &GenericParamDecl<'a>) {}
  fn visit_type_ref<'a>(&mut self, _type_ref: &TypeRef<'a>) {}
  fn visit_type_ref_after<'a>(&mut self, _type_ref: &TypeRef<'a>) {}
  fn visit_class_statement<'a>(&mut self, _class_statement: &ClassBodyStatement<'a>) {}
  fn visit_class_statement_after<'a>(&mut self, _class_statement: &ClassBodyStatement<'a>) {}
  fn visit_field_decl<'a>(&mut self, _field_decl: &FieldDef<'a>) {}
  fn visit_field_decl_after<'a>(&mut self, _field_decl: &FieldDef<'a>) {}
  fn visit_method_decl<'a>(&mut self, _method_decl: &MethodDecl<'a>) {}
  fn visit_method_decl_after<'a>(&mut self, _method_decl: &MethodDecl<'a>) {}
  fn visit_method_param<'a>(&mut self, _method_decl: &MethodParam<'a>) {}
  fn visit_method_param_after<'a>(&mut self, _method_decl: &MethodParam<'a>) {}
}

fn visit_type_program<'a>(program: TypeProgram<'a>, visitor: &mut impl TypeProgramVisitor) {
  fn visit_type_ref<'a>(type_ref: &TypeRef<'a>, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_type_ref(type_ref);

    match type_ref {
      TypeRef::ArrayTypeRef(arr) => visit_type_ref(&arr.array_type, visitor),
      TypeRef::SymTypeRef(sym) => {
        sym.params.as_ref().map(|param| {
          for type_ref_param in param {
            visit_type_ref(type_ref_param, visitor);
          }
        });
      }
    };

    visitor.visit_type_ref_after(type_ref);
  }

  fn visit_generic_param_decl<'a>(
    generic_param_decl: &GenericParamDecl<'a>,
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

  fn visit_field_decl<'a>(field_decl: &FieldDef<'a>, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_field_decl(field_decl);

    visit_type_ref(&field_decl.type_ref, visitor);

    visitor.visit_field_decl_after(field_decl);
  }

  fn visit_method_param<'a>(param: &MethodParam<'a>, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_method_param(param);

    visit_type_ref(&param.type_ref, visitor);

    visitor.visit_method_param_after(param);
  }

  fn visit_method_decl<'a>(method_decl: &MethodDecl<'a>, visitor: &mut impl TypeProgramVisitor) {
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

  fn visit_class_statement<'a>(
    class_statement: &ClassBodyStatement<'a>,
    visitor: &mut impl TypeProgramVisitor,
  ) {
    visitor.visit_class_statement(class_statement);

    match class_statement {
      ClassBodyStatement::FieldDecl(field_decl) => visit_field_decl(field_decl, visitor),
      ClassBodyStatement::MethodDecl(method_decl) => visit_method_decl(method_decl, visitor),
    }

    visitor.visit_class_statement_after(class_statement);
  }

  fn visit_class_decl<'a>(class_decl: &ClassDecl<'a>, visitor: &mut impl TypeProgramVisitor) {
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

  fn visit_program<'a>(program: &TypeProgram<'a>, visitor: &mut impl TypeProgramVisitor) {
    visitor.visit_program(&program);

    for class_decl in &program.expressions {
      visit_class_decl(class_decl, visitor);
    }

    visitor.visit_program_after(program);
  }

  visit_program(&program, visitor);
}

impl<'a> TypeProgram<'a> {
  pub fn visit(self, visitor: &mut impl TypeProgramVisitor) {
    visit_type_program(self, visitor);
  }
}
