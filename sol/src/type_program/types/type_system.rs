use crate::type_program::types::ObjectType;

pub struct TypeSystem {
  global: ObjectType,
}

impl TypeSystem {
  pub fn new(global: ObjectType) -> Self {
    Self { global }
  }
}
