use derive_getters::Getters;
use derive_new::new;
use enum_dispatch::enum_dispatch;
use serde::Serialize;
use std::{borrow::Cow, collections::HashMap};

use crate::type_program::{
  nodes::ast_node::{ASTNode, ASTNodeData},
  types::scope::Scope,
};

#[derive(Debug, Serialize, Clone, PartialEq)]
#[enum_dispatch(TypeImpl)]
pub enum Type {
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

  pub fn resolve_type(&self) {
    self.resolve_self_impl(self);
  }

  pub fn into_concrete<'a>(&'a self, scope: &Scope) -> Cow<'a, Type> {
    self.into_concrete_impl(self, scope)
  }
}

#[enum_dispatch]
pub trait TypeImpl {
  fn resolve_self_impl<'a>(&'a self, owner: &'a Type) -> Cow<'a, Type>;
  fn into_concrete_impl<'a>(&'a self, owner: &'a Type, scope: &Scope) -> Cow<'a, Type>;
}

#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct RefType {
  name: String,
  generic_params: Option<Vec<Type>>,
}

impl TypeImpl for RefType {
  fn resolve_self_impl<'a>(&'a self, _owner: &'a Type) -> Cow<'a, Type> {
    // This should be a concrete type at this point.
    todo!();
  }

  fn into_concrete_impl<'a>(&'a self, _owner: &'a Type, scope: &Scope) -> Cow<'a, Type> {
    Cow::Owned(
      RefType::new(
        scope.resolve_generic_name(&self.name()),
        self.generic_params().as_ref().map(|x| {
          x.iter()
            .map(|y| y.into_concrete(scope).as_ref().clone())
            .collect()
        }),
      )
      .into(),
    )
  }
}

/// Type for program arrays
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct ArrayType {
  arity: usize,
  ref_type: Box<Type>,
}

impl TypeImpl for ArrayType {
  fn resolve_self_impl<'a>(&'a self, owner: &'a Type) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }

  fn into_concrete_impl<'a>(&'a self, _owner: &'a Type, scope: &Scope) -> Cow<'a, Type> {
    Cow::Owned(
      ArrayType::new(
        *self.arity(),
        Box::new(self.ref_type().into_concrete(scope).as_ref().clone()),
      )
      .into(),
    )
  }
}

/// Type for methods
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct MethodType {
  params: Vec<Type>,
  generic_types: Option<Vec<Type>>,
  return_type: Option<Box<Type>>,
}

impl TypeImpl for MethodType {
  fn resolve_self_impl<'a>(&'a self, owner: &'a Type) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }

  fn into_concrete_impl<'a>(&'a self, owner: &'a Type, _scope: &Scope) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }
}

/// Type for method params
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct MethodParamType {
  ref_type: Box<Type>,
  is_variadic: bool,
}

impl TypeImpl for MethodParamType {
  fn resolve_self_impl<'a>(&'a self, _owner: &'a Type) -> Cow<'a, Type> {
    self.ref_type().resolve_self_impl(&self.ref_type())
  }

  fn into_concrete_impl<'a>(&'a self, owner: &'a Type, _scope: &Scope) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }
}

/// Type for generic declarations
/// Should not be instanced
/// Needs to be extended to account for inheritance for runtime binding checking
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct GenericType {
  name: String,
}

impl TypeImpl for GenericType {
  fn resolve_self_impl<'a>(&'a self, owner: &'a Type) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }

  fn into_concrete_impl<'a>(&'a self, owner: &'a Type, scope: &Scope) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }
}

/// Abstract type of objects
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct ObjectType {
  name: String,
  inherits: Option<Vec<Type>>,
  generic_params: Option<Vec<Type>>,
  body: Box<HashMap<String, Type>>,
}

impl TypeImpl for ObjectType {
  fn resolve_self_impl<'a>(&'a self, owner: &'a Type) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }

  fn into_concrete_impl<'a>(&'a self, owner: &'a Type, scope: &Scope) -> Cow<'a, Type> {
    todo!()
  }
}

/// The type of a field declaration
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct FieldType {
  identifier_type: Box<Type>,
  is_static: bool,
}

impl TypeImpl for FieldType {
  fn resolve_self_impl<'a>(&'a self, _owner: &'a Type) -> Cow<'a, Type> {
    self
      .identifier_type()
      .resolve_self_impl(&self.identifier_type())
  }

  fn into_concrete_impl<'a>(&'a self, owner: &'a Type, scope: &Scope) -> Cow<'a, Type> {
    self
      .identifier_type()
      .into_concrete_impl(self.identifier_type(), scope)
  }
}

/// The type of a type program
#[derive(new, Getters, Debug, Serialize, Clone, PartialEq)]
pub struct ProgramType {
  types: HashMap<String, Box<Type>>,
}

impl TypeImpl for ProgramType {
  fn resolve_self_impl<'a>(&'a self, owner: &'a Type) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }

  fn into_concrete_impl<'a>(&'a self, owner: &'a Type, scope: &Scope) -> Cow<'a, Type> {
    Cow::Borrowed(owner)
  }
}
