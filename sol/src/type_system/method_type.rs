use derive_getters::Getters;
use derive_new::new;
use serde::Serialize;

use crate::{type_program::GenericParamDecl, type_system::Type};

#[derive(new, Debug, Getters, Serialize)]
pub struct MethodParamType {
  inner: Type,
  is_variadic: bool,
}

#[derive(Debug, Getters, Serialize)]
pub struct MethodType {
  params: Vec<MethodParamType>,
  generic_params: Option<Vec<GenericParamDecl>>,
  return_type: Option<Box<Type>>,
}

impl MethodType {
  pub fn new(
    params: Vec<MethodParamType>,
    generic_params: Option<Vec<GenericParamDecl>>,
    return_type: Option<Type>,
  ) -> MethodType {
    MethodType {
      params,
      generic_params,
      return_type: return_type.map(|x| Box::new(x)),
    }
  }
}
