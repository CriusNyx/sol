mod class_decl;
mod generic_param_decl;
mod method_decl;
mod type_lexer;
mod type_program;
mod type_program_print;
mod type_ref;

use ariadne::{Color, Label, Report, ReportKind, Source};
use chumsky::prelude::*;
pub use class_decl::*;
pub use generic_param_decl::*;
use logos::Logos;
pub use method_decl::*;
pub use type_lexer::*;
pub use type_program::*;
pub use type_program_print::*;
pub use type_ref::*;

pub fn lex_type_program<'a>(source: &'a str) -> Result<Vec<TypeToken<'a>>, Vec<&'a str>> {
  let result = TypeToken::lexer(source).collect::<Result<Vec<TypeToken<'a>>, _>>();
  match result {
    Ok(vec) => Ok(vec),
    Err(_) => Err(vec![""]),
  }
}

pub fn parse_type_program<'a>(
  tokens: &'a Result<Vec<TypeToken<'a>>, Vec<&'a str>>,
  source: &'a str,
) -> Result<TypeProgram<'a>, Vec<&'a str>> {
  match tokens {
    Ok(vec) => {
      let parsed = type_parser::<'a>().parse(vec);
      match parsed.into_result() {
        Ok(program) => Ok(program),
        Err(errs) => {
          for err in errs {
            let err_range = err.span().into_range();
            let offending_tokens = &vec[err_range];
            let start = offending_tokens.first().unwrap().get_info().span.start;
            let end = offending_tokens.last().unwrap().get_info().span.end;

            dbg!(err.span().into_range());

            Report::build(ReportKind::Error, ("file.st", start..end))
              .with_config(ariadne::Config::new().with_index_type(ariadne::IndexType::Byte))
              .with_message(err.to_string())
              .with_code(-1)
              .with_label(
                Label::new(("file.st", start..end))
                  .with_message(err.reason().to_string())
                  .with_color(Color::Red),
              )
              .finish()
              .eprint(("file.st", Source::from(source)))
              .unwrap();
          }
          Err(vec!["Parse Error"])
        }
      }
    }
    Err(err) => Err(err.to_vec()),
  }
}
