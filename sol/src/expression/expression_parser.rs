use chumsky::prelude::*;
use derive_getters::Getters;
use derive_new::new;
use logos::Logos;
use serde::Serialize;
use ts_rs::TS;

use crate::expression::expression_token::ExpressionToken;

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub enum DerefExp {
  SymDeref(SymDeref),
}

impl From<SymDeref> for DerefExp {
  fn from(value: SymDeref) -> Self {
    Self::SymDeref(value)
  }
}

#[derive(new, Getters, Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct SymDeref {
  symbol: String,
  chain: Option<Box<DerefExp>>,
}

pub fn sym_deref_parser<'a>(
  deref_parser: impl Parser<'a, &'a [ExpressionToken], DerefExp, extra::Err<Rich<'a, ExpressionToken>>>
  + Clone,
) -> impl Parser<'a, &'a [ExpressionToken], SymDeref, extra::Err<Rich<'a, ExpressionToken>>> + Clone
{
  select! { ExpressionToken::DotOp(_) => () }
    .then(select! {ExpressionToken::Symbol(sym) => ExpressionToken::Symbol(sym)})
    .then(deref_parser.map(Some).or(empty().to(None)))
    .map(|((_, sym), chain)| SymDeref {
      symbol: sym.token_info().source.to_string(),
      chain: chain.map(|x| Box::new(x)),
    })
}

pub fn deref_parser<'a>()
-> impl Parser<'a, &'a [ExpressionToken], DerefExp, extra::Err<Rich<'a, ExpressionToken>>> + Clone {
  recursive(|deref_parser| {
    select! {ExpressionToken::Symbol(sym) => sym}
      .then(
        sym_deref_parser(deref_parser)
          .map(|x| Some(DerefExp::SymDeref(x)))
          .or(empty().to(None)),
      )
      .map(|(sym, chain)| {
        DerefExp::SymDeref(SymDeref {
          symbol: sym.source.to_string(),
          chain: chain.map(|x| Box::new(x)),
        })
      })
  })
}

#[derive(Debug)]
pub enum ExpressionParseError {
  LexError,
  ParseError,
}

pub fn parse_expression(source: String) -> Result<DerefExp, ExpressionParseError> {
  let tokens = ExpressionToken::lexer(&source)
    .collect::<Result<Vec<_>, _>>()
    .map_err(|_| ExpressionParseError::LexError);
  let parsed = tokens.and_then(|x| {
    deref_parser()
      .parse(&x)
      .into_result()
      .map_err(|_| ExpressionParseError::ParseError)
  });
  parsed
}
