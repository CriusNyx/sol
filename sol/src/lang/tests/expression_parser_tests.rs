#[cfg(test)]
mod expression_parser_tests {
  use chumsky::Parser;

  use crate::{
    helpers::program_equivalent::assert_equivalent,
    lang::{
      expression_parser::{reference_chain_parser, reference_sequence_sparse_parser},
      expression_token::{ExpressionToken, ExpressionTokenInfo, lex_expression},
      sol_ast::{
        deref_expression::ReferenceChain,
        expression_set::ExpressionSet,
        sol_ast::{SolAST, ToRc},
        symbol_expression::SymbolExpression,
      },
    },
  };

  #[test]
  pub fn can_parse_single_sym() {
    let tokens = lex_expression("symbol").unwrap();
    let actual = reference_chain_parser().parse(&tokens).unwrap();

    let expected: SolAST = ReferenceChain::new(vec![
      SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
        0..0,
        "symbol".to_string(),
        -1,
      )))
      .to_rc(),
    ])
    .into();

    assert_equivalent(&expected, &actual);
  }

  #[test]
  pub fn can_parse_sym_chain() {
    let tokens = lex_expression("symbol.field").unwrap();
    let actual = reference_chain_parser().parse(&tokens).unwrap();

    let expected: SolAST = ReferenceChain::new(vec![
      SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
        0..0,
        "symbol".to_string(),
        -1,
      )))
      .to_rc(),
      SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
        0..0,
        "field".to_string(),
        -1,
      )))
      .to_rc(),
    ])
    .into();

    assert_equivalent(&expected, &actual);
  }

  #[test]
  pub fn can_parse_empty_string() {
    let tokens = lex_expression("").unwrap();
    let actual = reference_sequence_sparse_parser().parse(&tokens).unwrap();

    let expected: SolAST = ExpressionSet::new(vec![]).into();

    assert_equivalent(&expected, &actual);
  }

  #[test]
  pub fn can_parse_single_references() {
    let tokens = lex_expression("symbol").unwrap();
    let actual = reference_sequence_sparse_parser().parse(&tokens).unwrap();

    let expected: SolAST = ExpressionSet::new(vec![
      ReferenceChain::new(vec![
        SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
          0..0,
          "symbol".to_string(),
          -1,
        )))
        .to_rc(),
      ])
      .to_rc(),
    ])
    .into();

    assert_equivalent(&expected, &actual);
  }

  #[test]
  pub fn can_parse_multiple_references() {
    let tokens = lex_expression("symbol symbol").unwrap();
    let actual = reference_sequence_sparse_parser().parse(&tokens).unwrap();

    let expected: SolAST = ExpressionSet::new(vec![
      ReferenceChain::new(vec![
        SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
          0..0,
          "symbol".to_string(),
          -1,
        )))
        .to_rc(),
      ])
      .to_rc(),
      ReferenceChain::new(vec![
        SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
          0..0,
          "symbol".to_string(),
          -1,
        )))
        .to_rc(),
      ])
      .to_rc(),
    ])
    .into();

    assert_equivalent(&expected, &actual);
  }

  #[test]
  pub fn can_parse_sparse_references() {
    let tokens = lex_expression("symbol + symbol").unwrap();
    let actual = reference_sequence_sparse_parser().parse(&tokens).unwrap();

    let expected: SolAST = ExpressionSet::new(vec![
      ReferenceChain::new(vec![
        SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
          0..0,
          "symbol".to_string(),
          -1,
        )))
        .to_rc(),
      ])
      .to_rc(),
      ReferenceChain::new(vec![
        SymbolExpression::new(ExpressionToken::Symbol(ExpressionTokenInfo::new(
          0..0,
          "symbol".to_string(),
          -1,
        )))
        .to_rc(),
      ])
      .to_rc(),
    ])
    .into();

    assert_equivalent(&expected, &actual);
  }
}
