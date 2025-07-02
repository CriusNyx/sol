use derive_getters::Getters;
use serde::Serialize;
use std::collections::HashMap;

use crate::type_system::{Scope, Type};

#[derive(Debug, Getters, Serialize)]
pub struct ObjectType {
  name: Option<String>,
  members: HashMap<String, Type>,
}

impl ObjectType {
  pub fn new(name: Option<String>) -> ObjectType {
    ObjectType {
      name: name,
      members: HashMap::new().into(),
    }
  }

  pub fn get_members_mut(&mut self) -> &mut HashMap<String, Type> {
    &mut self.members
  }
}

impl Scope for ObjectType {
  fn resolve_symbol(&self, symbol: &String) -> Option<&Type> {
    self.members().get(symbol)
  }
}
