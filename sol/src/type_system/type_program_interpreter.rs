use crate::{
  type_program::{
    ClassBodyStatement, ClassDecl, FieldDef, GlobalExp, GlobalVar, Identifier, MethodDecl,
    MethodParamDecl, TypeProgram, TypeRef,
  },
  type_system::{MethodParamType, MethodType, ObjectType, Type},
};

#[derive(Debug)]
pub enum InterpreterResult {
  Void,
  Identifier(Option<String>, Type),
}

impl InterpreterResult {
  pub fn extract_type(self) -> Option<Type> {
    match self {
      Self::Identifier(_, t) => Some(t),
      _ => None,
    }
  }
}

pub trait TypeProgramInterpreter {
  fn evaluate(&self) -> InterpreterResult;
}

impl TypeProgramInterpreter for TypeProgram {
  fn evaluate(&self) -> InterpreterResult {
    let mut program_type = ObjectType::new(None);
    let members = program_type.get_members_mut();

    for ele in &self.expressions {
      let evaluation = ele.evaluate();
      match evaluation {
        InterpreterResult::Identifier(Some(name), identifier_type) => {
          members.insert(name.to_string(), identifier_type);
        }
        InterpreterResult::Void => {}
        _ => {
          panic!("Could not evaluate type for expression",);
        }
      }
    }

    InterpreterResult::Identifier(None, Type::ObjectType(program_type))
  }
}

impl TypeProgramInterpreter for GlobalExp {
  fn evaluate(&self) -> InterpreterResult {
    match self {
      GlobalExp::GlobalVar(global_var) => global_var.evaluate(),
      GlobalExp::ClassDec(class_decl) => class_decl.evaluate(),
    }
  }
}

impl TypeProgramInterpreter for GlobalVar {
  fn evaluate(&self) -> InterpreterResult {
    self.identifier.evaluate()
  }
}

impl TypeProgramInterpreter for Identifier {
  fn evaluate(&self) -> InterpreterResult {
    InterpreterResult::Identifier(
      Some(self.identifier_name.to_string()),
      self.type_decl.evaluate().extract_type().unwrap(),
    )
  }
}

impl TypeProgramInterpreter for TypeRef {
  fn evaluate(&self) -> InterpreterResult {
    InterpreterResult::Identifier(None, Type::RefType(self.clone()))
  }
}

impl TypeProgramInterpreter for ClassDecl {
  fn evaluate(&self) -> InterpreterResult {
    let mut obj_type = ObjectType::new(Some(self.name.to_string()));
    let obj_members = obj_type.get_members_mut();

    for statement in self.body.iter().flatten() {
      match statement.evaluate() {
        InterpreterResult::Identifier(Some(name), statement_type) => {
          obj_members.insert(name, statement_type);
        }
        _ => panic!(),
      }
    }
    InterpreterResult::Identifier(Some(self.name.to_string()), obj_type.into())
  }
}

impl TypeProgramInterpreter for ClassBodyStatement {
  fn evaluate(&self) -> InterpreterResult {
    match self {
      ClassBodyStatement::FieldDecl(field) => field.evaluate(),
      ClassBodyStatement::MethodDecl(method) => method.evaluate(),
    }
  }
}

impl TypeProgramInterpreter for FieldDef {
  fn evaluate(&self) -> InterpreterResult {
    self.identifier.evaluate()
  }
}

impl TypeProgramInterpreter for MethodDecl {
  fn evaluate(&self) -> InterpreterResult {
    let method_type = MethodType::new(
      self
        .param_types
        .iter()
        .map(|param| param.into_param_type())
        .collect(),
      self.generic_params.clone(),
      self
        .return_type
        .as_ref()
        .map(|x| x.evaluate().extract_type().unwrap()),
    );

    InterpreterResult::Identifier(Some(self.name.to_string()), method_type.into())
  }
}

impl TypeProgramInterpreter for MethodParamDecl {
  fn evaluate(&self) -> InterpreterResult {
    self.type_ref.evaluate()
  }
}

impl MethodParamDecl {
  fn into_param_type(&self) -> MethodParamType {
    MethodParamType::new(self.evaluate().extract_type().unwrap(), self.variadic)
  }
}
