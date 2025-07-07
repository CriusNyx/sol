// pub trait TypeResolver {
//   fn resolve_type<T: Scope>(&self, scope: &T) -> TypeResolution;
// }

// impl TypeResolver for DerefExp {
//   fn resolve_type<T: Scope>(&self, scope: &T) -> TypeResolution {
//     match self {
//       Self::SymDeref(sym) => sym.resolve_type(scope),
//     }
//   }
// }

// impl TypeResolver for SymDeref {
//   fn resolve_type<T: Scope>(&self, scope: &T) -> TypeResolution {
//     todo!();
//     // let resolved_type = scope.resolve_symbol(self.symbol());
//     // match resolved_type {
//     //   Some(Type::RefType(ref_type)) => TypeResolution {
//     //     resolved_type: ref_type.clone(),
//     //   },
//     //   _ => {
//     //     panic!();
//     //   }
//     // }
//   }
// }

// pub fn apply_generic_params(
//   type_ref: &TypeRefAST,
//   generic_params: &HashMap<String, String>,
// ) -> TypeRefAST {
//   todo!()
// }
