mod sample_programs;
mod type_program;
mod wasm;
use type_program::*;

use crate::sample_programs::SAMPLE_PROGRAM_1;

fn main() {
  let tokens = lex_type_program(SAMPLE_PROGRAM_1);
  let ast = parse_type_program(&tokens, SAMPLE_PROGRAM_1);

  // Padding to make the program stick out.
  println!("");

  match ast {
    Ok(result) => {
      println!("{}", result.print_source());
    }
    Err(e) => {
      println!("Failed to parse {}", e.join("\n"));
    }
  };

  // Padding to make the program stick out.
  println!("");
}
