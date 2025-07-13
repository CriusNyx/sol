use std::{collections::HashMap, rc::Rc};

use derive_getters::Getters;
use derive_new::new;

use crate::type_program::types::{ArrayType, MethodParamType, MethodType, RefType, Type, TypeImpl};

#[derive(new, Clone, Getters, PartialEq, Debug)]
pub struct InstancedType {
  source: Rc<Type>,
  generic_args: Vec<Rc<Type>>,
}

#[derive(Debug)]
pub enum TypeResolutionError {
  CanOnlyResolveSymbolsForObjectTypes,
  CanOnlyCreateScopesForObjectTypes,
  SymbolNotInObjectOfType(SymbolNotInObjectOfType),
  SymNotFoundInGlobal(String),
}

#[derive(new, Getters, Debug)]
pub struct SymbolNotInObjectOfType {
  object_name: String,
  symbol: String,
}

impl InstancedType {
  pub fn plain(source: &Rc<Type>) -> InstancedType {
    InstancedType::new(source.clone(), vec![])
  }

  // Type should already have scope applied before this method is called.
  pub fn instance_type(
    source: &Rc<Type>,
    global_types: &HashMap<String, Rc<Type>>,
  ) -> InstancedType {
    match source.as_ref() {
      Type::ArrayType(_) => InstancedType::plain(source),
      Type::RefType(ref_type) => {
        let obj_type = global_types.get(ref_type.name()).unwrap();

        InstancedType::new(
          obj_type.clone(),
          ref_type
            .generic_params()
            .iter()
            .flatten()
            .cloned()
            .collect(),
        )
      }
      Type::MethodType(_) => InstancedType::plain(source),
      Type::MethodParamType(_) => panic!("Cannot instance a method param"),
      Type::GenericType(_) => panic!("Cannot instance a generic type"),
      Type::ObjectType(_) => InstancedType::plain(source),
      Type::FieldType(field_type) => {
        InstancedType::instance_type(field_type.identifier_type(), global_types)
      }
      Type::ProgramType(_) => InstancedType::plain(source),
    }
  }

  pub fn create_generic_param_scope(
    &self,
  ) -> Result<Rc<HashMap<String, Rc<Type>>>, TypeResolutionError> {
    match self.source().as_ref() {
      Type::ObjectType(obj_type) => Ok(Rc::new(
        obj_type
          .generic_params()
          .iter()
          .flatten()
          .map(|x| x.try_as_generic_type_ref().unwrap().name())
          .cloned()
          .zip(self.generic_args().iter().cloned())
          .collect::<HashMap<_, _>>(),
      )),
      Type::ProgramType(_) => Ok(Rc::new(HashMap::new())),
      _ => panic!("Cannot create generic param scope for type {:#?}", &self),
    }
  }

  fn array_type_apply_scope(array_type: &ArrayType, scope: &HashMap<String, Rc<Type>>) -> Rc<Type> {
    ArrayType::new(
      *array_type.arity(),
      Self::apply_type_scope(array_type.ref_type(), scope),
    )
    .to_rc()
  }

  fn type_ref_apply_scope(
    source: &Rc<Type>,
    ref_type: &RefType,
    scope: &HashMap<String, Rc<Type>>,
  ) -> Rc<Type> {
    match ref_type.generic_params() {
      Some(generic_params) => RefType::new(
        ref_type.name().to_string(),
        Some(
          generic_params
            .iter()
            .map(|x| Self::apply_type_scope(x, scope))
            .collect(),
        ),
      )
      .to_rc(),

      None => scope
        .get(ref_type.name())
        .map_or_else(|| source.clone(), |x| x.clone()),
    }
  }

  fn method_type_apply_scope(
    method_type: &MethodType,
    scope: &HashMap<String, Rc<Type>>,
  ) -> Rc<Type> {
    MethodType::new(
      method_type
        .params()
        .iter()
        .map(|x| Self::apply_type_scope(x, scope))
        .collect(),
      method_type
        .generic_types()
        .as_ref()
        .map(|x| x.iter().map(|y| Self::apply_type_scope(y, scope)).collect()),
      method_type
        .return_type()
        .as_ref()
        .map(|x| Self::apply_type_scope(x, scope)),
    )
    .to_rc()
  }

  fn method_param_type_apply_scope(
    method_params_type: &MethodParamType,
    scope: &HashMap<String, Rc<Type>>,
  ) -> Rc<Type> {
    MethodParamType::new(
      Self::apply_type_scope(method_params_type.ref_type(), scope),
      *method_params_type.is_variadic(),
    )
    .to_rc()
  }

  pub fn apply_type_scope(source: &Rc<Type>, scope: &HashMap<String, Rc<Type>>) -> Rc<Type> {
    match source.as_ref() {
      Type::ArrayType(array_type) => Self::array_type_apply_scope(array_type, scope),
      Type::RefType(ref_type) => Self::type_ref_apply_scope(source, ref_type, scope),
      Type::MethodType(method_type) => Self::method_type_apply_scope(method_type, scope),
      Type::MethodParamType(method_params_type) => {
        Self::method_param_type_apply_scope(method_params_type, scope)
      }
      Type::GenericType(_) => source.clone(),
      Type::ObjectType(_) => source.clone(),
      Type::FieldType(_) => todo!(),
      Type::ProgramType(_) => todo!(),
    }
  }

  fn unwrap_sym(source: &Rc<Type>) -> &Rc<Type> {
    match source.as_ref() {
      Type::ArrayType(_) => source,
      Type::RefType(_) => source,
      Type::MethodType(_) => source,
      Type::MethodParamType(_) => panic!("Cannot unwrap a method param type"),
      Type::GenericType(_) => panic!("Cannot unwrap a generic type"),
      Type::ObjectType(_) => source,
      Type::FieldType(field_type) => Self::unwrap_sym(field_type.identifier_type()),
      Type::ProgramType(_) => panic!("Cannot unwrap program type"),
    }
  }

  pub fn resolve_sym(
    &self,
    symbol: &str,
    global_types: &HashMap<String, Rc<Type>>,
  ) -> Result<InstancedType, TypeResolutionError> {
    let sym_type = match self.source().as_ref() {
      Type::ObjectType(_) => {
        let Some(obj_type) = self.source().try_as_object_type_ref() else {
          return Err(TypeResolutionError::CanOnlyResolveSymbolsForObjectTypes);
        };

        let Some(value) = obj_type.body().get(symbol) else {
          return Err(TypeResolutionError::SymbolNotInObjectOfType(
            SymbolNotInObjectOfType::new(obj_type.name().to_string(), symbol.to_string()),
          ));
        };
        value
      }
      Type::ProgramType(_) => match global_types.get(symbol) {
        Some(result) => result,
        None => return Err(TypeResolutionError::SymNotFoundInGlobal(symbol.to_string())),
      },
      _ => panic!("Cannot resolve symbol for object \n{:#?}", self.source()),
    };

    let sym_type = Self::unwrap_sym(sym_type);

    let scope = match self.create_generic_param_scope() {
      Ok(val) => val,
      Err(err) => return Err(err),
    };

    dbg!(sym_type);

    Ok(Self::instance_type(
      &Self::apply_type_scope(sym_type, &scope),
      global_types,
    ))
  }

  pub fn resolve_chain(
    &self,
    symbols: &[&str],
    global_types: &HashMap<String, Rc<Type>>,
  ) -> Result<InstancedType, TypeResolutionError> {
    let mut current = self.clone();
    for sym in symbols {
      match current.resolve_sym(sym, global_types) {
        Ok(val) => current = val,
        Err(err) => return Err(err),
      }
    }
    Ok(current)
  }
}
