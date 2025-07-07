#[cfg(test)]
mod type_system_tests {
  use chumsky::Parser;
  use logos::Logos;
  use std::collections::HashMap;

  use crate::{
    type_program::{
      nodes::ast_node::ASTNodeData,
      parser::{
        field_parser, identifier_decl_parser, method_parser, type_decl_parser, type_program_parser,
        type_ref_decl_parser,
      },
      type_system::*,
    },
    type_program_old::TypeToken,
  };

  #[test]
  fn can_calc_type_ref() {
    let source = "String";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    dbg!(&parsed);

    let parsed_type = parsed.calc_type(None);

    let expected: (Option<String>, Type) = (None, RefType::new("String".to_string(), None).into());

    assert_eq!(expected, parsed_type);
  }

  #[test]
  fn can_calc_type_ref_with_param() {
    let source = "IEnumerable<String>";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    dbg!(&parsed);

    let parsed_type = parsed.calc_type(None);

    let expected: (Option<String>, Type) = (
      None,
      RefType::new(
        "IEnumerable".to_string(),
        Some(vec![RefType::new("String".to_string(), None).into()]),
      )
      .into(),
    );

    assert_eq!(expected, parsed_type);
  }

  #[test]
  fn can_calc_type_ref_with_multi_param() {
    let source = "IDictionary<Int, String>";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    dbg!(&parsed);

    let parsed_type = parsed.calc_type(None);

    let expected: (Option<String>, Type) = (
      None,
      RefType::new(
        "IDictionary".to_string(),
        Some(vec![
          RefType::new("Int".to_string(), None).into(),
          RefType::new("String".to_string(), None).into(),
        ]),
      )
      .into(),
    );

    assert_eq!(expected, parsed_type);
  }

  #[test]
  fn sym_name_resolves_correctly() {
    let source = "IDictionary<Int, String>";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    parsed.calc_type(None);

    let result = parsed
      .data()
      .try_as_type_ref_decl_ref()
      .unwrap()
      .name()
      .get_type();

    let expected: (Option<String>, Type) = (
      Some("IDictionary".to_string()),
      RefType::new(
        "IDictionary".to_string(),
        Some(vec![
          RefType::new("Int".to_string(), None).into(),
          RefType::new("String".to_string(), None).into(),
        ]),
      )
      .into(),
    );

    assert_eq!(expected, result);
  }

  #[test]
  fn can_calc_arr_type() {
    let source = "String[]";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    let expected: (Option<String>, Type) = (
      None,
      ArrayType::new(1, Box::new(RefType::new("String".to_string(), None).into())).into(),
    );

    assert_eq!(expected, result);
  }

  #[test]
  fn can_calc_arr_type_with_arity() {
    let source = "String[,]";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    let expected: (Option<String>, Type) = (
      None,
      ArrayType::new(2, Box::new(RefType::new("String".to_string(), None).into())).into(),
    );

    assert_eq!(expected, result);
  }

  #[test]
  fn can_calc_lambda_type() {
    let source = "() => void";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!((None, MethodType::new(vec![], None, None).into()), result);
  }

  #[test]
  fn can_calc_lambda_type_with_param() {
    let source = "(String) => void";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        None,
        MethodType::new(
          vec![
            MethodParamType::new(
              Box::new(RefType::new("String".to_string(), None).into()),
              false
            )
            .into()
          ],
          None,
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_lambda_type_with_multi_param() {
    let source = "(String, String) => void";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        None,
        MethodType::new(
          vec![
            MethodParamType::new(
              Box::new(RefType::new("String".to_string(), None).into()),
              false
            )
            .into(),
            MethodParamType::new(
              Box::new(RefType::new("String".to_string(), None).into()),
              false
            )
            .into(),
          ],
          None,
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_lambda_type_with_generic_param() {
    let source = "<T>() => void";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        None,
        MethodType::new(
          vec![],
          Some(vec![GenericType::new("T".to_string()).into()]),
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_lambda_type_with_multi_generic_param() {
    let source = "<T, U>() => void";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        None,
        MethodType::new(
          vec![],
          Some(vec![
            GenericType::new("T".to_string()).into(),
            GenericType::new("U".to_string()).into()
          ]),
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_lambda_type_with_return() {
    let source = "() => String";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        None,
        MethodType::new(
          vec![],
          None,
          Some(Box::new(RefType::new("String".to_string(), None).into()))
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_method_type() {
    let source = "Method();";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        Some("Method".to_string()),
        MethodType::new(vec![], None, None).into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_method_type_with_param() {
    let source = "Method(String);";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        Some("Method".to_string()),
        MethodType::new(
          vec![
            MethodParamType::new(
              Box::new(RefType::new("String".to_string(), None).into()),
              false
            )
            .into()
          ],
          None,
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_method_type_with_multi_param() {
    let source = "Method(String, String);";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        Some("Method".to_string()),
        MethodType::new(
          vec![
            MethodParamType::new(
              Box::new(RefType::new("String".to_string(), None).into()),
              false
            )
            .into(),
            MethodParamType::new(
              Box::new(RefType::new("String".to_string(), None).into()),
              false
            )
            .into()
          ],
          None,
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_method_type_with_generic_param() {
    let source = "Method<T>();";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        Some("Method".to_string()),
        MethodType::new(
          vec![],
          Some(vec![GenericType::new("T".to_string()).into()]),
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_method_type_with_multi_generic_param() {
    let source = "Method<T, U>();";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        Some("Method".to_string()),
        MethodType::new(
          vec![],
          Some(vec![
            GenericType::new("T".to_string()).into(),
            GenericType::new("U".to_string()).into()
          ]),
          None
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn can_calc_method_type_with_return() {
    let source = "Method(): String;";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let result = parsed.calc_type(None);

    assert_eq!(
      (
        Some("Method".to_string()),
        MethodType::new(
          vec![],
          None,
          Some(Box::new(RefType::new("String".to_string(), None).into()))
        )
        .into()
      ),
      result
    );
  }

  #[test]
  fn method_name_resolves_correctly() {
    let source = "Method();";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    parsed.calc_type(None);

    let method_name_type = parsed
      .data()
      .try_as_method_decl_ref()
      .unwrap()
      .name()
      .get_type();

    assert_eq!(
      (
        Some("Method".to_string()),
        MethodType::new(vec![], None, None).into()
      ),
      method_name_type
    );
  }

  #[test]
  fn can_calc_identifier_type() {
    let source = "ident: String";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = identifier_decl_parser().parse(&tokens).unwrap();

    let actual = parsed.calc_type(None);

    assert_eq!(
      (
        Some("ident".to_string()),
        RefType::new("String".to_string(), None).into()
      ),
      actual
    )
  }

  #[test]
  fn identifier_name_resolves_correctly() {
    let source = "ident: String";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = identifier_decl_parser().parse(&tokens).unwrap();

    parsed.calc_type(None);

    let identifier_type = parsed
      .data()
      .try_as_identifier_decl_ref()
      .unwrap()
      .name()
      .get_type();

    assert_eq!(
      (
        Some("ident".to_string()),
        RefType::new("String".to_string(), None).into()
      ),
      identifier_type
    )
  }

  #[test]
  fn can_calc_field_decl_type() {
    let source = "ident: String;";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = field_parser().parse(&tokens).unwrap();

    let actual = parsed.calc_type(None);

    assert_eq!(
      (
        Some("ident".to_string()),
        FieldType::new(
          Box::new(RefType::new("String".to_string(), None).into()),
          false
        )
        .into()
      ),
      actual
    )
  }

  #[test]
  fn can_calc_type_decl_type() {
    let source = "type Class { field: String; Method(): String; }";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_decl_parser().parse(&tokens).unwrap();

    let actual = parsed.calc_type(None);

    assert_eq!(
      (
        Some("Class".to_string()),
        ObjectType::new(
          "Class".to_string(),
          None,
          None,
          HashMap::<String, Box<Type>>::from([
            (
              "field".to_string(),
              Box::new(
                FieldType::new(
                  Box::new(RefType::new("String".to_string(), None).into()),
                  false
                )
                .into()
              )
            ),
            (
              "Method".to_string(),
              Box::new(
                MethodType::new(
                  vec![],
                  None,
                  Some(Box::new(RefType::new("String".to_string(), None).into()))
                )
                .into()
              )
            )
          ])
        )
        .into()
      ),
      actual
    )
  }

  #[test]
  fn can_calc_type_program_type() {
    let source = "static staticField: String;
    type Class { field: String; Method(): String; }";
    let tokens = TypeToken::lexer(source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_program_parser().parse(&tokens).unwrap();

    let actual = parsed.calc_type(None);

    assert_eq!(
      (
        None,
        ProgramType::new(HashMap::from([
          (
            "staticField".to_string(),
            Box::new(RefType::new("String".to_string(), None).into())
          ),
          (
            "Class".to_string(),
            Box::new(
              ObjectType::new(
                "Class".to_string(),
                None,
                None,
                HashMap::<String, Box<Type>>::from([
                  (
                    "field".to_string(),
                    Box::new(
                      FieldType::new(
                        Box::new(RefType::new("String".to_string(), None).into()),
                        false
                      )
                      .into()
                    )
                  ),
                  (
                    "Method".to_string(),
                    Box::new(
                      MethodType::new(
                        vec![],
                        None,
                        Some(Box::new(RefType::new("String".to_string(), None).into()))
                      )
                      .into()
                    )
                  )
                ])
              )
              .into()
            )
          )
        ]))
        .into()
      ),
      actual
    )
  }
}
