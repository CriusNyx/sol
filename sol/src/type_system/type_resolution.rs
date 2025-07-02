use derive_getters::Getters;
use derive_new::new;

use crate::{
  expression::expression_parser::{DerefExp, SymDeref},
  type_program::TypeRef,
  type_system::Type,
};

#[derive(new, Getters, Debug)]
pub struct TypeResolution {
  resolved_type: TypeRef,
}

pub trait Scope {
  fn resolve_symbol(&self, symbol: &String) -> Option<&Type>;
}

pub trait TypeResolver {
  fn resolve_type<T: Scope>(&self, scope: &T) -> TypeResolution;
}

impl TypeResolver for DerefExp {
  fn resolve_type<T: Scope>(&self, scope: &T) -> TypeResolution {
    match self {
      Self::SymDeref(sym) => sym.resolve_type(scope),
    }
  }
}

impl TypeResolver for SymDeref {
  fn resolve_type<T: Scope>(&self, scope: &T) -> TypeResolution {
    let resolved_type = scope.resolve_symbol(self.symbol());
    match resolved_type {
      Some(Type::RefType(ref_type)) => TypeResolution {
        resolved_type: ref_type.clone(),
      },
      _ => {
        panic!();
      }
    }
  }
}
