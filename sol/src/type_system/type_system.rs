use derive_getters::Getters;
use derive_new::new;
use serde::Serialize;

use crate::{
  type_program::TypeRef,
  type_system::{MethodType, ObjectType},
};

#[derive(Debug, Serialize)]
pub enum Type {
  ObjectType(ObjectType),
  MethodType(MethodType),
  RefType(TypeRef),
}

#[derive(new, Getters, Debug, Serialize)]
pub struct RefType {
  name: String,
}

impl From<ObjectType> for Type {
  fn from(value: ObjectType) -> Self {
    Type::ObjectType(value)
  }
}

impl From<MethodType> for Type {
  fn from(value: MethodType) -> Self {
    Type::MethodType(value)
  }
}

impl From<TypeRef> for Type {
  fn from(value: TypeRef) -> Self {
    Type::RefType(value)
  }
}
