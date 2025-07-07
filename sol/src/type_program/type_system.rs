use std::collections::HashMap;

use derive_getters::Getters;
use derive_more::From;
use derive_new::new;
use serde::Serialize;

use crate::type_program::nodes::ast_node::{ASTNode, ASTNodeData};

#[derive(From, Debug, Serialize, Clone, PartialEq)]
pub enum Type {
  Never,
  ArrayType(ArrayType),
  RefType(RefType),
  MethodType(MethodType),
  MethodParamType(MethodParamType),
  GenericType(GenericType),
  ObjectType(ObjectType),
  FieldType(FieldType),
  ProgramType(ProgramType),
}

impl Type {
  pub fn from_method(
    params: &Vec<ASTNode>,
    generic_params: &Option<Vec<ASTNode>>,
    return_type: &Option<Box<ASTNode>>,
  ) -> Type {
    let param_types = params
      .iter()
      .map(|x| x.calc_type(None).1)
      .collect::<Vec<_>>();
    let generic_types = generic_params
      .as_ref()
      .map(|x| x.iter().map(|y| y.calc_type(None).1).collect::<Vec<_>>());
    let return_type = return_type.as_ref().map(|x| Box::new(x.calc_type(None).1));

    MethodType::new(param_types, generic_types, return_type).into()
  }
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct RefType {
  name: String,
  generic_params: Option<Vec<Type>>,
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct ArrayType {
  arity: usize,
  ref_type: Box<Type>,
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct MethodType {
  params: Vec<Type>,
  generic_types: Option<Vec<Type>>,
  return_type: Option<Box<Type>>,
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct MethodParamType {
  ref_type: Box<Type>,
  is_variadic: bool,
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct GenericType {
  name: String,
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct ObjectType {
  name: String,
  inherits: Option<Vec<Type>>,
  generic_params: Option<Vec<Type>>,
  body: HashMap<String, Box<Type>>,
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct FieldType {
  identifier_type: Box<Type>,
  is_static: bool,
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct ProgramType {
  types: HashMap<String, Box<Type>>,
}
