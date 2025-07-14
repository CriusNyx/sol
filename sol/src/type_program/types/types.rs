use derive_getters::Getters;
use derive_more::From;
use derive_new::new;
use serde::Serialize;
use std::{collections::HashMap, rc::Rc};
use strum_macros::EnumTryAs;

use crate::type_program::nodes::st_ast::{ASTNodeData, StAst};

#[derive(From, Debug, Clone, PartialEq, EnumTryAs)]
pub enum Type {
  ArrayType(ArrayType),
  RefType(RefType),
  MethodType(MethodType),
  MethodOverloadType(MethodOverloadType),
  MethodParamType(MethodParamType),
  GenericType(GenericType),
  ObjectType(ObjectType),
  FieldType(FieldType),
  ProgramType(ProgramType),
}

impl Type {
  pub fn from_method_overload(
    params: &Vec<StAst>,
    generic_params: &Option<Vec<StAst>>,
    return_type: &Option<Box<StAst>>,
  ) -> Type {
    let param_types = params
      .iter()
      .map(|x| x.calc_type(None).1)
      .map(Rc::new)
      .collect::<Vec<_>>();
    let generic_types = generic_params.as_ref().map(|x| {
      x.iter()
        .map(|y| y.calc_type(None).1)
        .map(Rc::new)
        .collect::<Vec<_>>()
    });
    let return_type = return_type.as_ref().map(|x| Rc::new(x.calc_type(None).1));

    MethodType::new(vec![
      MethodOverloadType::new(param_types, generic_types, return_type).to_rc(),
    ])
    .into()
  }
}

pub trait TypeImpl {
  fn to_rc(self) -> Rc<Type>;
  fn to_some_rc(self) -> Option<Rc<Type>>;
}

impl<T: Into<Type>> TypeImpl for T {
  fn to_rc(self) -> Rc<Type> {
    Rc::new(self.into())
  }

  fn to_some_rc(self) -> Option<Rc<Type>> {
    Some(self.to_rc())
  }
}

#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct RefType {
  name: String,
  generic_params: Option<Vec<Rc<Type>>>,
}

/// Type for program arrays
#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct ArrayType {
  arity: usize,
  ref_type: Rc<Type>,
}

/// Type for methods
#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct MethodType {
  overloads: Vec<Rc<Type>>,
}

impl MethodType {
  pub fn overloads_mut(&mut self) -> &mut Vec<Rc<Type>> {
    &mut self.overloads
  }
}

/// Type for method overload
#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct MethodOverloadType {
  params: Vec<Rc<Type>>,
  generic_types: Option<Vec<Rc<Type>>>,
  return_type: Option<Rc<Type>>,
}

/// Type for method params
#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct MethodParamType {
  ref_type: Rc<Type>,
  is_variadic: bool,
}

/// Type for generic declarations
/// Should not be instanced
/// Needs to be extended to account for inheritance for runtime binding checking
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct GenericType {
  name: String,
}

/// Abstract type of objects
#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct ObjectType {
  name: String,
  inherits: Option<Vec<Rc<Type>>>,
  generic_params: Option<Vec<Rc<Type>>>,
  body: HashMap<String, Rc<Type>>,
}

/// The type of a field declaration
#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct FieldType {
  identifier_type: Rc<Type>,
  is_static: bool,
}

/// The type of a type program
#[derive(new, Getters, Debug, Clone, PartialEq)]
pub struct ProgramType {
  types: Rc<HashMap<String, Rc<Type>>>,
}
