#![allow(dead_code)]
#![allow(unused_variables)]

mod lsp;
mod sample_programs;
mod type_program;
mod wasm;

use crate::sample_programs::SAMPLE_PROGRAM_1;
use ariadne::{Color, Label, Report, ReportKind, Source};
use type_program::*;

fn main() {
  let tokens = lex_type_program(SAMPLE_PROGRAM_1);
  let ast = parse_type_program(&tokens);

  // Padding to make the program stick out.
  println!("");

  match ast {
    Ok(result) => {
      dbg!(&result);
      println!("{}", result.print_source());
    }
    Err(CompileError::LexError) => {
      println!("Failed to lex program");
    }
    Err(CompileError::ParseError(parse_error)) => {
      for err in parse_error.parse_errors {
        let err_range = err.span().into_range();
        let offending_tokens = &parse_error.tokens[err_range];
        let start = offending_tokens.first().unwrap().get_info().span.start;
        let end = offending_tokens.last().unwrap().get_info().span.end;

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
          .eprint(("file.st", Source::from(SAMPLE_PROGRAM_1)))
          .unwrap();
      }
    }
  };

  // Padding to make the program stick out.
  println!("");
}
