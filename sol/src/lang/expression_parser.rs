use chumsky::prelude::*;

use crate::lang::{
  expression_token::ExpressionToken,
  sol_ast::{
    deref_expression::ReferenceChain,
    expression_set::ExpressionSet,
    sol_ast::{SolAST, ToRc},
    symbol_expression::SymbolExpression,
  },
};

pub enum MapUnit<T> {
  Unit(T),
  Vec(Vec<T>),
}

pub fn map_unit<T, U>(
  input: Vec<T>,
  unit_fn: impl FnOnce(&T) -> U,
  vec_fn: impl FnOnce(Vec<T>) -> U,
) -> U {
  match input.iter().count() {
    1 => unit_fn(input.first().unwrap()),
    _ => vec_fn(input),
  }
}

pub fn symbol_parser<'a>()
-> impl Parser<'a, &'a [ExpressionToken], SolAST, extra::Err<Rich<'a, ExpressionToken>>> + Clone {
  select! { ExpressionToken::Symbol(info) => ExpressionToken::Symbol(info)}
    .map(|x| SymbolExpression::new(x).into())
}

pub fn reference_chain_unit_parser<'a>()
-> impl Parser<'a, &'a [ExpressionToken], SolAST, extra::Err<Rich<'a, ExpressionToken>>> + Clone {
  symbol_parser()
}

pub fn reference_chain_parser<'a>()
-> impl Parser<'a, &'a [ExpressionToken], SolAST, extra::Err<Rich<'a, ExpressionToken>>> + Clone {
  reference_chain_unit_parser()
    .separated_by(select! {ExpressionToken::DotOp(_)})
    .at_least(1)
    .collect::<Vec<_>>()
    .map(|vec| ReferenceChain::new(vec.iter().map(|el| el.clone().to_rc()).collect()).into())
}

pub fn reference_chain_chuff_parser<'a>()
-> impl Parser<'a, &'a [ExpressionToken], (), extra::Err<Rich<'a, ExpressionToken>>> + Clone {
  select! {
    ExpressionToken::OpenParen(_),
    ExpressionToken::ClosedParen(_),
    ExpressionToken::OpenAngle(_),
    ExpressionToken::ClosedAngle(_),

    ExpressionToken::AddOp(_),
    ExpressionToken::MinusOp(_),
    ExpressionToken::MulOp(_),
    ExpressionToken::DivOp(_)
  }
  .repeated()
}

pub fn reference_sequence_sparse_parser<'a>()
-> impl Parser<'a, &'a [ExpressionToken], SolAST, extra::Err<Rich<'a, ExpressionToken>>> + Clone {
  reference_chain_parser()
    .then_ignore(reference_chain_chuff_parser())
    .repeated()
    .collect::<Vec<_>>()
    .map(|vec| ExpressionSet::new(vec.iter().map(|el| el.clone().to_rc()).collect()).into())
}
