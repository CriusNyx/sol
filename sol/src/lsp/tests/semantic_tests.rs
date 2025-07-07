#[cfg(test)]
mod semantic_test {
  use crate::lsp::{
    analyze_program_semantics_internal,
    semantic_types::{SemanticToken, SemanticType},
  };

  #[test]
  fn semantics_work_for_empty_program() {
    let src = "";
    let actual = analyze_program_semantics_internal(&src);

    let expected: Vec<SemanticToken> = vec![];

    assert_eq!(expected, actual);
  }

  #[test]
  fn semantics_work_for_program_that_doesnt_lex() {
    let src = "@!^@#$";
    let actual = analyze_program_semantics_internal(&src);

    let expected: Vec<SemanticToken> = vec![];

    assert_eq!(expected, actual);
  }

  #[test]
  fn semantics_work_for_program_that_doesnt_parse() {
    let src = "type String";
    let actual = analyze_program_semantics_internal(&src);

    let expected: Vec<SemanticToken> = vec![
      SemanticToken::new(SemanticType::Keyword, 0, 4, 4),
      SemanticToken::new(SemanticType::Variable, 5, 11, 6),
    ];

    assert_eq!(expected, actual);
  }

  #[test]
  fn semantics_work_for_type_decl() {
    let src = "type String;";
    let actual = analyze_program_semantics_internal(&src);

    let expected: Vec<SemanticToken> = vec![
      SemanticToken::new(SemanticType::Keyword, 0, 4, 4),
      SemanticToken::new(SemanticType::Type, 5, 11, 6),
    ];

    assert_eq!(expected, actual);
  }

  #[test]
  fn semantics_work_for_global_decl() {
    let src = "static val: String;";
    let actual = analyze_program_semantics_internal(&src);

    let expected: Vec<SemanticToken> = vec![
      SemanticToken::new(SemanticType::Keyword, 0, 6, 6),
      SemanticToken::new(SemanticType::Variable, 7, 10, 3),
      SemanticToken::new(SemanticType::Type, 12, 18, 6),
    ];

    assert_eq!(expected, actual);
  }

  #[test]
  fn semantics_work_for_field() {
    let src = "type Test { field: String; }";
    let actual = analyze_program_semantics_internal(&src);

    let expected: Vec<SemanticToken> = vec![
      SemanticToken::new(SemanticType::Keyword, 0, 4, 4),
      SemanticToken::new(SemanticType::Type, 5, 9, 4),
      SemanticToken::new(SemanticType::Variable, 12, 17, 5),
      SemanticToken::new(SemanticType::Type, 19, 25, 6),
    ];

    assert_eq!(expected, actual);
  }

  #[test]
  fn semantics_work_for_method() {
    let src = "type Test { Foo(); }";
    let actual = analyze_program_semantics_internal(&src);

    let expected: Vec<SemanticToken> = vec![
      SemanticToken::new(SemanticType::Keyword, 0, 4, 4),
      SemanticToken::new(SemanticType::Type, 5, 9, 4),
      SemanticToken::new(SemanticType::Method, 12, 15, 3),
    ];

    assert_eq!(expected, actual);
  }
}
