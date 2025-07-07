#[cfg(test)]
mod parser_tests {
  use chumsky::Parser;
  use logos::Logos;

  use crate::{
    type_program::{
      nodes::{
        array_decl::ArrayDecl,
        ast_node::{ASTNode, ASTNodeData, ToAST},
        field_decl::FieldDecl,
        generic_param_decl::GenericParamDecl,
        global_decl::GlobalDecl,
        identifier::IdentifierDecl,
        lambda_decl::LambdaDecl,
        method_decl::MethodDecl,
        method_param_decl::MethodParamDecl,
        symbol_node::SymbolNode,
        type_decl::TypeDecl,
        type_program_node::TypeProgramNode,
        type_ref_decl::TypeRefDecl,
        unit_decl::UnitDecl,
      },
      parser::{
        field_parser, generic_param_parser, global_decl_parser, identifier_decl_parser,
        method_parser, type_decl_parser, type_program_parser, type_ref_decl_parser,
      },
      program_equivalent::ProgramEquivalent,
    },
    type_program_old::TypeToken,
  };

  fn assert_program_equivalent(expected: &ASTNode, parsed: &ASTNode) {
    assert!(
      expected.program_equivalent(&parsed),
      "expected = {expected:#?},\n parsed = {parsed:#?}",
    );
  }

  fn assert_program_format(expected: &str, parsed: &ASTNode) {
    assert_eq!(expected, parsed.format_source(),);
  }

  #[test]
  fn can_parse_type_unit() {
    let source = "(String)";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = UnitDecl::new(
      TypeRefDecl::new(
        SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
        None,
      )
      .to_ast_boxed_debug(),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("(String)", &parsed);
  }

  #[test]
  fn can_parse_type_sym() {
    let source = "String";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = TypeRefDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("String", &parsed);
  }

  #[test]
  fn can_parse_array() {
    let source = "String[]";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    dbg!(&parsed);

    let expected = ArrayDecl::new(
      1,
      TypeRefDecl::new(
        SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
        None,
      )
      .to_ast_boxed_debug(),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("String[]", &parsed);
  }

  #[test]
  fn can_parse_array_with_arity() {
    let source = "String[,]";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    dbg!(&parsed);

    let expected = ArrayDecl::new(
      2,
      TypeRefDecl::new(
        SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
        None,
      )
      .to_ast_boxed_debug(),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("String[,]", &parsed);
  }

  #[test]
  fn can_parse_nested_array() {
    let source = "String[][]";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = ArrayDecl::new(
      1,
      ArrayDecl::new(
        1,
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_boxed_debug(),
      )
      .to_ast_boxed_debug(),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("String[][]", &parsed);
  }

  #[test]
  fn can_parse_type_ref_with_generic_param() {
    let source = "IEnumerable<String>";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = TypeRefDecl::new(
      SymbolNode::new("IEnumerable".to_string()).to_ast_boxed_debug(),
      Some(vec![
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_debug(),
      ]),
    )
    .to_ast_boxed_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("IEnumerable<String>", &parsed);
  }

  #[test]
  fn can_parse_type_ref_with_multi_generic_param() {
    let source = "IDictionary<String, String>";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = TypeRefDecl::new(
      SymbolNode::new("IDictionary".to_string()).to_ast_boxed_debug(),
      Some(vec![
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_debug(),
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_debug(),
      ]),
    )
    .to_ast_boxed_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("IDictionary<String, String>", &parsed);
  }

  #[test]
  fn can_parse_identifier() {
    let source = "identifier: String";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = identifier_decl_parser().parse(&tokens).unwrap();

    let expected = IdentifierDecl::new(
      SymbolNode::new("identifier".to_string()).to_ast_boxed_debug(),
      TypeRefDecl::new(
        SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
        None,
      )
      .to_ast_boxed_debug(),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("identifier: String", &parsed);
  }

  #[test]
  fn can_parse_lambda() {
    let source = "() => void";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(None, vec![], None).to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("() => void", &parsed);
  }

  #[test]
  fn can_parse_lambda_with_param() {
    let source = "(String) => void";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(
      None,
      vec![
        MethodParamDecl::new(
          Box::new(
            TypeRefDecl::new(
              SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
              None,
            )
            .to_ast_debug(),
          ),
          false,
        )
        .to_ast_debug(),
      ],
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("(String) => void", &parsed);
  }

  #[test]
  fn can_parse_lambda_with_multi_param() {
    let source = "(String, String) => void";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(
      None,
      vec![
        MethodParamDecl::new(
          Box::new(
            TypeRefDecl::new(
              SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
              None,
            )
            .to_ast_debug(),
          ),
          false,
        )
        .to_ast_debug(),
        MethodParamDecl::new(
          Box::new(
            TypeRefDecl::new(
              SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
              None,
            )
            .to_ast_debug(),
          ),
          false,
        )
        .to_ast_debug(),
      ],
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("(String, String) => void", &parsed);
  }

  #[test]
  fn can_parse_lambda_with_variadic() {
    let source = "(...String[]) => void";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(
      None,
      vec![
        MethodParamDecl::new(
          ArrayDecl::new(
            1,
            TypeRefDecl::new(
              SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
              None,
            )
            .to_ast_boxed_debug(),
          )
          .to_ast_boxed_debug(),
          true,
        )
        .to_ast_debug(),
      ],
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("(...String[]) => void", &parsed);
  }

  #[test]
  fn can_parse_lambda_with_return() {
    let source = "() => String";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(
      None,
      vec![],
      Some(
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_boxed_debug(),
      ),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("() => String", &parsed);
  }

  #[test]
  fn can_parse_lambda_arr_return() {
    let source = "() => String[]";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(
      None,
      vec![],
      Some(
        ArrayDecl::new(
          1,
          TypeRefDecl::new(
            SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
            None,
          )
          .to_ast_boxed_debug(),
        )
        .to_ast_boxed_debug(),
      ),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("() => String[]", &parsed);
  }

  #[test]
  fn can_parse_lambda_arr() {
    let source = "(() => void)[]";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = ArrayDecl::new(
      1,
      UnitDecl::new(LambdaDecl::new(None, vec![], None).to_ast_boxed_debug()).to_ast_boxed_debug(),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("(() => void)[]", &parsed);
  }

  #[test]
  fn can_parse_lambda_generic_param() {
    let source = "<T>() => void";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(
      Some(vec![
        GenericParamDecl::new(SymbolNode::new("T".to_string()).to_ast_boxed_debug(), None)
          .to_ast_debug(),
      ]),
      vec![],
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("<T>() => void", &parsed);
  }

  #[test]
  fn can_parse_lambda_multi_generic_param() {
    let source = "<T, U>() => void";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_ref_decl_parser().parse(&tokens).unwrap();

    let expected = LambdaDecl::new(
      Some(vec![
        GenericParamDecl::new(SymbolNode::new("T".to_string()).to_ast_boxed_debug(), None)
          .to_ast_debug(),
        GenericParamDecl::new(SymbolNode::new("U".to_string()).to_ast_boxed_debug(), None)
          .to_ast_debug(),
      ]),
      vec![],
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("<T, U>() => void", &parsed);
  }

  #[test]
  fn can_parse_generic_param() {
    let source = "String";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = generic_param_parser(type_ref_decl_parser())
      .parse(&tokens)
      .unwrap();

    let expected = GenericParamDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("String", &parsed);
  }

  #[test]
  fn can_parse_generic_param_inherits() {
    let source = "String: IEnumerable<char>";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = generic_param_parser(type_ref_decl_parser())
      .parse(&tokens)
      .unwrap();

    let expected = GenericParamDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      Some(vec![
        TypeRefDecl::new(
          SymbolNode::new("IEnumerable".to_string()).to_ast_boxed_debug(),
          Some(vec![
            TypeRefDecl::new(
              SymbolNode::new("char".to_string()).to_ast_boxed_debug(),
              None,
            )
            .to_ast_debug(),
          ]),
        )
        .to_ast_debug(),
      ]),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("String: IEnumerable<char>", &parsed);
  }

  #[test]
  fn can_parse_generic_param_multi_inherits() {
    let source = "String: IEnumerable<char> + IDisposable";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = generic_param_parser(type_ref_decl_parser())
      .parse(&tokens)
      .unwrap();

    let expected = GenericParamDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      Some(vec![
        TypeRefDecl::new(
          SymbolNode::new("IEnumerable".to_string()).to_ast_boxed_debug(),
          Some(vec![
            TypeRefDecl::new(
              SymbolNode::new("char".to_string()).to_ast_boxed_debug(),
              None,
            )
            .to_ast_debug(),
          ]),
        )
        .to_ast_debug(),
        TypeRefDecl::new(
          SymbolNode::new("IDisposable".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_debug(),
      ]),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("String: IEnumerable<char> + IDisposable", &parsed);
  }

  #[test]
  fn can_parse_field_decl() {
    let source = "name: String;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = field_parser().parse(&tokens).unwrap();

    let expected = FieldDecl::new(
      IdentifierDecl::new(
        SymbolNode::new("name".to_string()).to_ast_boxed_debug(),
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_boxed_debug(),
      )
      .to_ast_boxed_debug(),
      false,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("name: String;", &parsed);
  }

  #[test]
  fn can_parse_static_field_decl() {
    let source = "static name: String;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = field_parser().parse(&tokens).unwrap();

    let expected = FieldDecl::new(
      IdentifierDecl::new(
        SymbolNode::new("name".to_string()).to_ast_boxed_debug(),
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_boxed_debug(),
      )
      .to_ast_boxed_debug(),
      true,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("static name: String;", &parsed);
  }

  #[test]
  fn can_parse_type() {
    let source = "type String;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_decl_parser().parse(&tokens).unwrap();

    let expected = TypeDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      None,
      None,
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("type String;", &parsed);
  }

  #[test]
  fn can_parse_type_generic() {
    let source = "type String<T>;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_decl_parser().parse(&tokens).unwrap();

    let expected = TypeDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      Some(vec![
        GenericParamDecl::new(SymbolNode::new("T".to_string()).to_ast_boxed_debug(), None)
          .to_ast_debug(),
      ]),
      None,
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("type String<T>;", &parsed);
  }

  #[test]
  fn can_parse_type_inherits() {
    let source = "type String: IEnumerable;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_decl_parser().parse(&tokens).unwrap();

    let expected = TypeDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      None,
      Some(vec![
        TypeRefDecl::new(
          SymbolNode::new("IEnumerable".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_debug(),
      ]),
      None,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("type String: IEnumerable;", &parsed);
  }

  #[test]
  fn can_parse_type_body() {
    let source = "type String { length: int; }";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_decl_parser().parse(&tokens).unwrap();

    let expected = TypeDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      None,
      None,
      Some(vec![
        FieldDecl::new(
          IdentifierDecl::new(
            SymbolNode::new("length".to_string()).to_ast_boxed_debug(),
            TypeRefDecl::new(
              SymbolNode::new("int".to_string()).to_ast_boxed_debug(),
              None,
            )
            .to_ast_boxed_debug(),
          )
          .to_ast_boxed_debug(),
          false,
        )
        .to_ast_debug(),
      ]),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("type String {\nlength: int;\n}", &parsed);
  }

  #[test]
  fn can_parse_type_with_method() {
    let source = "type String { Method(); }";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_decl_parser().parse(&tokens).unwrap();

    let expected = TypeDecl::new(
      SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
      None,
      None,
      Some(vec![
        MethodDecl::new(
          SymbolNode::new("Method".to_string()).to_ast_boxed_debug(),
          None,
          vec![],
          None,
          false,
        )
        .to_ast_debug(),
      ]),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("type String {\nMethod();\n}", &parsed);
  }

  #[test]
  fn can_parse_method() {
    let source = "Method();";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let expected = MethodDecl::new(
      SymbolNode::new("Method".to_string()).to_ast_boxed_debug(),
      None,
      vec![],
      None,
      false,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
  }

  #[test]
  fn can_parse_method_with_params() {
    let source = "Method(String);";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let expected = MethodDecl::new(
      SymbolNode::new("Method".to_string()).to_ast_boxed_debug(),
      None,
      vec![
        MethodParamDecl::new(
          TypeRefDecl::new(
            SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
            None,
          )
          .to_ast_boxed_debug(),
          false,
        )
        .to_ast_debug(),
      ],
      None,
      false,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("Method(String);", &parsed);
  }

  #[test]
  fn can_parse_method_with_variadic_params() {
    let source = "Method(...String);";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let expected = MethodDecl::new(
      SymbolNode::new("Method".to_string()).to_ast_boxed_debug(),
      None,
      vec![
        MethodParamDecl::new(
          TypeRefDecl::new(
            SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
            None,
          )
          .to_ast_boxed_debug(),
          true,
        )
        .to_ast_debug(),
      ],
      None,
      false,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("Method(...String);", &parsed);
  }

  #[test]
  fn can_parse_method_with_generic_param() {
    let source = "Method<T>();";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let expected = MethodDecl::new(
      SymbolNode::new("Method".to_string()).to_ast_boxed_debug(),
      Some(vec![
        GenericParamDecl::new(SymbolNode::new("T".to_string()).to_ast_boxed_debug(), None)
          .to_ast_debug(),
      ]),
      vec![],
      None,
      false,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("Method<T>();", &parsed);
  }

  #[test]
  fn can_parse_method_with_void_return() {
    let source = "Method(): void;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let expected = MethodDecl::new(
      SymbolNode::new("Method".to_string()).to_ast_boxed_debug(),
      None,
      vec![],
      None,
      false,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("Method();", &parsed);
  }

  #[test]
  fn can_parse_method_with_return() {
    let source = "Method(): String;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = method_parser().parse(&tokens).unwrap();

    let expected = MethodDecl::new(
      SymbolNode::new("Method".to_string()).to_ast_boxed_debug(),
      None,
      vec![],
      Some(
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_boxed_debug(),
      ),
      false,
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("Method(): String;", &parsed);
  }

  #[test]
  fn can_parse_global_exp() {
    let source = "static name: String;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = global_decl_parser().parse(&tokens).unwrap();

    let expected = GlobalDecl::new(
      IdentifierDecl::new(
        SymbolNode::new("name".to_string()).to_ast_boxed_debug(),
        TypeRefDecl::new(
          SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
          None,
        )
        .to_ast_boxed_debug(),
      )
      .to_ast_boxed_debug(),
    )
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("static name: String;", &parsed);
  }

  #[test]
  fn can_parse_type_program() {
    let source = "";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_program_parser().parse(&tokens).unwrap();

    let expected = TypeProgramNode::new(vec![]).to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("", &parsed);
  }

  #[test]
  fn can_parse_type_program_with_global() {
    let source = "static name: String;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_program_parser().parse(&tokens).unwrap();

    let expected = TypeProgramNode::new(vec![
      GlobalDecl::new(
        IdentifierDecl::new(
          SymbolNode::new("name".to_string()).to_ast_boxed_debug(),
          TypeRefDecl::new(
            SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
            None,
          )
          .to_ast_boxed_debug(),
        )
        .to_ast_boxed_debug(),
      )
      .to_ast_debug(),
    ])
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("static name: String;", &parsed);
  }

  #[test]
  fn can_parse_type_program_with_type() {
    let source = "type String;";
    let tokens = TypeToken::lexer(&source)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();

    let parsed = type_program_parser().parse(&tokens).unwrap();

    let expected = TypeProgramNode::new(vec![
      TypeDecl::new(
        SymbolNode::new("String".to_string()).to_ast_boxed_debug(),
        None,
        None,
        None,
      )
      .to_ast_debug(),
    ])
    .to_ast_debug();

    assert_program_equivalent(&expected, &parsed);
    assert_program_format("type String;", &parsed);
  }
}
