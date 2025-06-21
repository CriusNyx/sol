use std::ops::Range;

use chumsky::{extra::ParserExtra, input::MapExtra, span::SimpleSpan};
use serde::Serialize;
use ts_rs::TS;

use crate::type_program::TypeToken;

#[derive(Debug, Clone, Serialize, TS)]
#[ts(export)]
pub struct TokenSource<'token> {
  pub token_range: Range<usize>,
  pub tokens: &'token [TypeToken<'token>],
}

impl TokenSource<'_> {
  pub fn from_extra<'token, 'a, E: ParserExtra<'token, &'token [TypeToken<'token>]>>(
    extra: &mut MapExtra<'token, 'a, &'token [TypeToken<'token>], E>,
  ) -> TokenSource<'token> {
    TokenSource {
      token_range: (extra.span() as SimpleSpan).into_range(),
      tokens: extra.slice(),
    }
  }
}
