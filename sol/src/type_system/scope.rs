// use derive_getters::Getters;
// use derive_new::new;

// use crate::type_system::{ObjectType, Type};

// pub trait Scope {
//   fn resolve_symbol(&self, symbol: &String) -> Option<&Type>;
// }

// #[derive(new, Getters)]
// pub struct ObjectScope<'a> {
//   object_type: &'a ObjectType,
// }

// impl ObjectType {
//   pub fn into_scope<'a>(&'a self) -> ObjectScope<'a> {
//     ObjectScope::new(self)
//   }
// }

// impl<'a> Scope for ObjectScope<'a> {
//   fn resolve_symbol(&self, symbol: &String) -> Option<&Type> {
//     todo!();
//     // self.object_type.members().get(symbol)
//   }
// }
