#[cfg(test)]
mod parser_tests {

  const TEST_PROGRAM: &str = "type string: IEnumerable<char> + Array<char>[];";

  use chumsky::Parser;
  use logos::Logos;
  use std::cell::Cell;
  use strum::IntoDiscriminant;

  use crate::type_program::{
    nodes::st_ast::{NodeData, NodeDataDiscriminants},
    st_parser::type_program_parser,
    st_token::StToken,
  };

  #[test]

  fn traverse_ast_works_correctly() {
    let sym_count = Cell::new(0);

    let lexons = StToken::lexer(&TEST_PROGRAM)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let program = type_program_parser().parse(&lexons).unwrap();

    program.traverse(&mut |x| match x.data() {
      NodeData::SymbolNode(_) => {
        sym_count.set(sym_count.get() + 1);
      }
      _ => {}
    });

    assert_eq!(5, sym_count.get())
  }

  #[test]
  fn collect_ast_works_correctly() {
    let lexons = StToken::lexer(&TEST_PROGRAM)
      .map(|x| x.unwrap())
      .collect::<Vec<_>>();
    let program = type_program_parser().parse(&lexons).unwrap();

    let debug_tokens = program
      .collect()
      .iter()
      .map(|x| x.data().discriminant())
      .collect::<Vec<_>>();

    assert_eq!(
      vec![
        NodeDataDiscriminants::TypeProgramNode,
        NodeDataDiscriminants::TypeDecl,
        NodeDataDiscriminants::TypeName,
        NodeDataDiscriminants::SymbolNode,
        NodeDataDiscriminants::TypeRefDecl,
        NodeDataDiscriminants::TypeName,
        NodeDataDiscriminants::SymbolNode,
        NodeDataDiscriminants::TypeRefDecl,
        NodeDataDiscriminants::TypeName,
        NodeDataDiscriminants::SymbolNode,
        NodeDataDiscriminants::ArrayDecl,
        NodeDataDiscriminants::TypeRefDecl,
        NodeDataDiscriminants::TypeName,
        NodeDataDiscriminants::SymbolNode,
        NodeDataDiscriminants::TypeRefDecl,
        NodeDataDiscriminants::TypeName,
        NodeDataDiscriminants::SymbolNode
      ] as Vec<NodeDataDiscriminants>,
      debug_tokens
    );
  }
}
