#![allow(dead_code)]
#![allow(unused_variables)]

mod expression;
mod helpers;
mod lsp;
mod sample_programs;
mod type_context;
mod type_program;
mod type_program_old;
mod type_system;
mod wasm;

use crate::{
  expression::expression_parser::parse_expression,
  sample_programs::{SAMPLE_PROGRAM_1, SAMPLE_PROGRAM_3},
};
use ariadne::{Color, Label, Report, ReportKind, Source};
use type_program_old::*;

fn main() {
  let tokens = lex_type_program(SAMPLE_PROGRAM_1);
  let ast = parse_type_program(&tokens);
  let ast_result = ast.as_ref().ok().clone();
  // let type_system = evaluate_type_program(&ast);
  // let root = type_system.as_ref().unwrap();

  let exp = parse_expression("value".to_string());
  // let resolved = exp.map(|x| x.resolve_type(&root.into_scope()));

  // dbg!(resolved);

  // Padding to make the program stick out.
  println!("");

  println!(
    "{}",
    ast_result
      .map(|x| x.print_source())
      .unwrap_or("".to_string())
  );

  // Padding to make the program stick out.
  println!("");

  // match type_system {
  //   Ok(result) => {
  //     // dbg!(&result);
  //   }
  //   Err(CompileError::LexError) => {
  //     println!("Failed to lex program");
  //   }
  //   Err(CompileError::ParseError(parse_error)) => {
  //     for err in parse_error.parse_errors {
  //       let err_range = err.span().into_range();
  //       let offending_tokens = &parse_error.tokens[err_range];
  //       let start = offending_tokens.first().unwrap().get_info().span.start;
  //       let end = offending_tokens.last().unwrap().get_info().span.end;

  //       Report::build(ReportKind::Error, ("file.st", start..end))
  //         .with_config(ariadne::Config::new().with_index_type(ariadne::IndexType::Byte))
  //         .with_message(err.to_string())
  //         .with_code(-1)
  //         .with_label(
  //           Label::new(("file.st", start..end))
  //             .with_message(err.reason().to_string())
  //             .with_color(Color::Red),
  //         )
  //         .finish()
  //         .eprint(("file.st", Source::from(SAMPLE_PROGRAM_3)))
  //         .unwrap();
  //     }
  //   }
  //   _ => {}
  // };

  // Padding to make the program stick out.
  println!("");
}
