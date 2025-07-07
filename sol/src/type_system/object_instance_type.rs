// use std::collections::HashMap;

// use derive_getters::Getters;

// use crate::type_system::{ObjectType, Type};

// #[derive(Getters)]
// pub struct ObjectInstanceType<'a> {
//   object_type: &'a ObjectType,
//   generic_params_resolution: HashMap<String, &'a Type>,
// }

// impl<'a> ObjectInstanceType<'a> {
//   pub fn new(object_type: &'a ObjectType) -> Self {
//     ObjectInstanceType {
//       object_type,
//       generic_params_resolution: HashMap::new(),
//     }
//   }

//   pub fn get_generic_params_mut(&'a mut self) -> &'a mut HashMap<String, &'a Type> {
//     &mut self.generic_params_resolution
//   }
// }
