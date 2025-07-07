use derive_getters::Getters;
use derive_new::new;
use serde::Serialize;

use crate::type_program_old::{
  ArrayTypeRefAST, GenericParamDecl, LambdaDeclAST, MethodParamDecl, SymTypeRefAST, TypeRefAST,
};

#[derive(new, Getters, Clone, Debug, Serialize)]
pub struct ArrayTypeRef {
  arity: u16,
  array_type: Box<TypeRef>,
}

#[derive(new, Getters, Clone, Debug, Serialize)]
pub struct SymTypeRef {
  name: String,
  params: Option<Vec<TypeRef>>,
}

#[derive(new, Getters, Clone, Debug, Serialize)]
pub struct LambdaParam {
  type_ref: TypeRef,
  variadic: bool,
}

#[derive(new, Getters, Clone, Debug, Serialize)]
pub struct LambdaTypeRef {
  param_types: Vec<LambdaParam>,
  generic_params: Option<Vec<GenericParamDecl>>,
  return_type: Option<Box<TypeRef>>,
}

#[derive(Debug, Clone, Serialize)]
pub enum TypeRef {
  ArrayTypeRef(ArrayTypeRef),
  SymTypeRef(SymTypeRef),
  LambdaTypeRef(LambdaTypeRef),
}

impl From<&TypeRefAST> for TypeRef {
  fn from(value: &TypeRefAST) -> Self {
    match value {
      TypeRefAST::ArrayTypeRef(array) => TypeRef::ArrayTypeRef(array.into()),
      TypeRefAST::SymTypeRef(sym) => TypeRef::SymTypeRef(sym.into()),
      TypeRefAST::LambdaDecl(lambda) => TypeRef::LambdaTypeRef(lambda.into()),
    }
  }
}

impl From<&ArrayTypeRefAST> for ArrayTypeRef {
  fn from(value: &ArrayTypeRefAST) -> Self {
    ArrayTypeRef::new(
      value.arity,
      Box::new(TypeRef::from(value.array_type.as_ref())),
    )
  }
}

impl From<&SymTypeRefAST> for SymTypeRef {
  fn from(value: &SymTypeRefAST) -> Self {
    SymTypeRef {
      name: value.name.to_string(),
      params: value
        .params
        .as_ref()
        .map(|x| x.iter().map(|y| TypeRef::from(y)).collect::<Vec<_>>()),
    }
  }
}

impl From<&LambdaDeclAST> for LambdaTypeRef {
  fn from(value: &LambdaDeclAST) -> Self {
    LambdaTypeRef::new(
      value
        .param_types
        .iter()
        .map(|x| x.into())
        .collect::<Vec<_>>(),
      value.generic_params.clone(),
      value
        .return_type
        .as_ref()
        .map(|x| Box::new(TypeRef::from(x.as_ref()))),
    )
  }
}

impl From<&MethodParamDecl> for LambdaParam {
  fn from(value: &MethodParamDecl) -> Self {
    LambdaParam::new(TypeRef::from(&value.type_ref), value.variadic)
  }
}
