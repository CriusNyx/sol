use std::collections::HashMap;

use crate::type_program::types::ProgramType;

pub struct Scope<'program_type> {
  type_program: &'program_type ProgramType,
  generic_types: HashMap<String, String>,
  parent: Option<Box<Scope<'program_type>>>,
}

impl<'program_type> From<&'program_type ProgramType> for Scope<'program_type> {
  fn from(type_program: &'program_type ProgramType) -> Self {
    Self::new(type_program, HashMap::new(), None)
  }
}

impl<'program_type> Scope<'program_type> {
  fn new(
    type_program: &'program_type ProgramType,
    generic_types: HashMap<String, String>,
    parent: Option<Box<Scope<'program_type>>>,
  ) -> Scope<'program_type> {
    Scope {
      type_program,
      generic_types,
      parent,
    }
  }

  pub fn push(self, types: HashMap<String, String>) -> Scope<'program_type> {
    Self::new(self.type_program, types, Some(self.into()))
  }

  pub fn to_global(self) -> Scope<'program_type> {
    match self.parent {
      Some(val) => val.to_global(),
      None => self,
    }
  }

  pub fn to_global_ref(&self) -> &Scope<'program_type> {
    match self.parent.as_ref() {
      Some(val) => val.to_global_ref(),
      None => self,
    }
  }

  pub fn resolve_generic_name(&self, name: &str) -> String {
    match self.generic_types.get(name) {
      Some(val) => self.resolve_generic_name(val),
      None => self
        .parent
        .as_ref()
        .map_or_else(|| name.to_string(), |x| x.resolve_generic_name(name)),
    }
  }
}
