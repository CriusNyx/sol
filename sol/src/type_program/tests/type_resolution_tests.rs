#[cfg(test)]
mod type_resolution_tests {
  use std::collections::HashMap;

  use crate::type_program::{
    type_program::TypeProgram,
    types::{
      MethodOverloadType, MethodParamType, MethodType, ObjectType, RefType, TypeImpl,
      type_resolution::InstancedType,
    },
  };

  #[test]
  fn can_resolve_global_method() {
    let parsed_program =
      TypeProgram::parse_string("type String; static global: () => String;").unwrap();
    let global_types = parsed_program.get_global_types();
    let global_instance = parsed_program.get_global_instance();

    let expected = InstancedType::plain(
      &MethodType::new(vec![
        MethodOverloadType::new(
          vec![],
          None,
          RefType::new("String".to_string(), None).to_some_rc(),
        )
        .to_rc(),
      ])
      .to_rc(),
    );
    let actual = global_instance
      .resolve_sym("global", &global_types)
      .unwrap();
    assert_eq!(expected, actual);
  }

  #[test]
  fn can_resolve_global_field() {
    let parsed_program = TypeProgram::parse_string("type String; static global: String;").unwrap();
    let global_types = parsed_program.get_global_types();
    let global_instance = parsed_program.get_global_instance();

    let expected = InstancedType::plain(
      &ObjectType::new("String".to_string(), None, None, HashMap::new()).to_rc(),
    );
    let actual = global_instance
      .resolve_sym("global", &global_types)
      .unwrap();
    assert_eq!(expected, actual);
  }

  #[test]
  fn can_resolve_sym_for_field() {
    let parsed_program =
      TypeProgram::parse_string("type FieldType{} type Object { field: FieldType; }").unwrap();

    let global_types = parsed_program.get_global_types();

    let object_type = InstancedType::new(global_types.get("Object").unwrap().clone(), vec![]);
    let field_type = global_types.get("FieldType").unwrap().clone();

    let expected = InstancedType::new(field_type, vec![]);
    let actual = object_type.resolve_sym("field", &global_types).unwrap();
    assert_eq!(expected, actual);
  }

  #[test]
  fn can_resolve_sym_for_method() {
    let parsed_program = TypeProgram::parse_string("type Object { Foo(); }").unwrap();

    let global_types = parsed_program.get_global_types();

    let object_type = InstancedType::new(global_types.get("Object").unwrap().clone(), vec![]);

    let expected = InstancedType::new(
      MethodType::new(vec![MethodOverloadType::new(vec![], None, None).to_rc()]).to_rc(),
      vec![],
    );
    let actual = object_type.resolve_sym("Foo", &global_types).unwrap();

    assert_eq!(expected, actual);
  }

  #[test]
  fn can_resolve_sym_for_method_overload() {
    let parsed_program =
      TypeProgram::parse_string("type String; type Object { Foo(); Foo(String); }").unwrap();

    let global_types = parsed_program.get_global_types();

    let object_type = InstancedType::new(global_types.get("Object").unwrap().clone(), vec![]);

    let expected = InstancedType::new(
      MethodType::new(vec![
        MethodOverloadType::new(vec![], None, None).to_rc(),
        MethodOverloadType::new(
          vec![
            MethodParamType::new(RefType::new("String".to_string(), None).to_rc(), false).to_rc(),
          ],
          None,
          None,
        )
        .to_rc(),
      ])
      .to_rc(),
      vec![],
    );
    let actual = object_type.resolve_sym("Foo", &global_types).unwrap();

    assert_eq!(expected, actual);
  }

  #[test]
  fn can_resolve_generic_field() {
    let parsed_program = TypeProgram::parse_string(
      "type String; type Generic<T>{ field: T; } static instance: Generic<String>;",
    )
    .unwrap();
    let global_types = parsed_program.get_global_types();
    let global_instance = parsed_program.get_global_instance();

    let expected = InstancedType::new(
      ObjectType::new("String".to_string(), None, None, HashMap::new()).to_rc(),
      vec![],
    );
    let actual = global_instance
      .resolve_chain(&["instance", "field"], &global_types)
      .unwrap();

    assert_eq!(expected, actual)
  }

  #[test]
  fn can_resolve_object_type() {
    let parsed_program = TypeProgram::parse_string("type Object;").unwrap();
    let global_types = parsed_program.get_global_types();
    let global_instance = parsed_program.get_global_instance();

    let expected = InstancedType::new(
      ObjectType::new("Object".to_string(), None, None, HashMap::new()).to_rc(),
      vec![],
    );
    let actual = global_instance
      .resolve_sym("Object", &global_types)
      .unwrap();

    assert_eq!(expected, actual);
  }

  #[test]
  fn can_resolve_recursive_reference() {
    let parsed_program = TypeProgram::parse_string(
      "type LinkedList<T>{ current: T; next: LinkedList<T>; } static head: LinkedList<String>;",
    )
    .unwrap();
    let global_types = parsed_program.get_global_types();
    let global_instance = parsed_program.get_global_instance();

    let expected = global_instance.resolve_sym("head", &global_types).unwrap();
    let actual1 = global_instance
      .resolve_chain(&["head", "next"], &global_types)
      .unwrap();
    let actual2 = global_instance
      .resolve_chain(&["head", "next", "next"], &global_types)
      .unwrap();

    assert_eq!(expected, actual1);
    assert_eq!(expected, actual2);
  }

  #[test]
  fn can_resolve_nested_reference() {
    let parsed_program =
      TypeProgram::parse_string("type A { b: B; } type B { c: C; } type C; static obj: A;")
        .unwrap();

    let global_types = parsed_program.get_global_types();
    let global_instance = parsed_program.get_global_instance();

    let expected = global_instance.resolve_sym("C", &global_types).unwrap();
    let actual = global_instance
      .resolve_chain(&["obj", "b", "c"], &global_types)
      .unwrap();

    assert_eq!(expected, actual);
  }
}
