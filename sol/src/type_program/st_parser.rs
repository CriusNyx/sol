use std::ops::Range;

use chumsky::{extra::ParserExtra, input::MapExtra, prelude::*};

use crate::type_program::{
  nodes::{
    array_decl::ArrayDecl,
    field_decl::FieldDecl,
    generic_param_decl::GenericParamDecl,
    global_decl::GlobalDecl,
    identifier::IdentifierDecl,
    lambda_decl::LambdaDecl,
    method_decl::MethodDecl,
    method_param_decl::MethodParamDecl,
    st_ast::{StAst, ToAST},
    symbol_node::SymbolNode,
    type_decl::TypeDecl,
    type_program_node::TypeProgramNode,
    type_ref_decl::TypeRefDecl,
    unit_decl::UnitDecl,
  },
  st_token::StToken,
};

pub fn symbol_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  select! {StToken::Symbol(info) => StToken::Symbol(info)}
    .map_with(|x, e| SymbolNode::new(x.get_info().source.to_string()).to_ast(range(e)))
}

pub fn type_program_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  top_level_statement_parser()
    .repeated()
    .collect::<Vec<_>>()
    .map_with(|x, e| TypeProgramNode::new(x).to_ast(range(e)))
}

pub fn top_level_statement_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  choice((global_decl_parser(), type_decl_parser()))
}

pub fn global_decl_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  select! {StToken::StaticKeyword(_)}
    .then(identifier_decl_parser())
    .then_ignore(select! {StToken::Semicolon(_)})
    .map_with(|(_, ident), e| GlobalDecl::new(Box::new(ident)).to_ast(range(e)))
}

pub fn field_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  select! {StToken::StaticKeyword(_)}
    .to(true)
    .or(empty().to(false))
    .then(identifier_decl_parser())
    .then_ignore(select! {StToken::Semicolon(_)})
    .map_with(|(is_static, identifier), e| {
      FieldDecl::new(Box::new(identifier), is_static).to_ast(range(e))
    })
}

pub fn method_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  // Static keyword
  select! {StToken::StaticKeyword(_)}
    .to(true)
    .or(empty().to(false))
    // Name
    .then(symbol_parser())
    // Generic Params
    .then(generic_param_set_parser(type_ref_decl_parser()))
    // Method Params
    .then(method_param_set_parser(type_ref_decl_parser()))
    // Return Type
    .then(choice((
      select! {StToken::Colon(_)}
        .then(type_ref_decl_parser())
        .map(|(_, x)| Some(Box::new(x))),
      select! {StToken::Colon(_)}
        .then_ignore(select! {StToken::VoidKeyword(_)})
        .to(None),
      empty().to(None),
    )))
    .then_ignore(select! {StToken::Semicolon(_)})
    .map_with(
      |((((is_static, name), generic_params), method_params), return_type), e| {
        MethodDecl::new(
          Box::new(name),
          generic_params,
          method_params,
          return_type,
          is_static,
        )
        .to_ast(range(e))
      },
    )
}

pub fn body_statement_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  choice((field_parser(), method_parser()))
}

pub fn type_decl_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  // Type Keyword
  select! {StToken::TypeKeyword(_)}
    // Symbol Parser
    .then(symbol_parser())
    // Generic params
    .then(generic_param_set_parser(type_ref_decl_parser()))
    // Inherits
    .then(
      select! {StToken::Colon(_)}
        .then(
          type_ref_decl_parser()
            .separated_by(select! {StToken::AddOp(_)})
            .collect::<Vec<_>>(),
        )
        .map(|(_, vec)| vec)
        .or_not(),
    )
    // Body
    .then(
      body_statement_parser()
        .repeated()
        .collect::<Vec<_>>()
        .delimited_by(
          select! { StToken::OpenCurly(_) },
          select! { StToken::ClosedCurly(_)},
        )
        .map(Some)
        .or(select! {StToken::Semicolon(_)}.to(None)),
    )
    .map_with(|((((_, sym), generic_param), inherits), body), e| {
      TypeDecl::new(Box::new(sym), generic_param, inherits, body).to_ast(range(e))
    })
}

pub fn generic_param_set_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone,
) -> impl Parser<'a, &'a [StToken], Option<Vec<StAst>>, extra::Err<Rich<'a, StToken>>> + Clone {
  generic_param_parser(type_ref_decl_parser)
    .separated_by(select! {StToken::Comma(_)})
    .collect::<Vec<_>>()
    .delimited_by(
      select! {StToken::OpenCaret(_)},
      select! {StToken::ClosedCaret(_)},
    )
    .or_not()
}

pub fn generic_param_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone,
) -> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  symbol_parser()
    .then(
      select! {StToken::Colon(_)}
        .then(
          type_ref_decl_parser
            .separated_by(select! {StToken::AddOp(_)})
            .collect::<Vec<_>>(),
        )
        .map(|(_, inherits)| inherits)
        .or_not(),
    )
    .map_with(|(sym, inherits), e| GenericParamDecl::new(Box::new(sym), inherits).to_ast(range(e)))
}

pub fn method_param_set_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone,
) -> impl Parser<'a, &'a [StToken], Vec<StAst>, extra::Err<Rich<'a, StToken>>> + Clone {
  method_param_parser(type_ref_decl_parser)
    .separated_by(select! {StToken::Comma(_)})
    .collect::<Vec<_>>()
    .delimited_by(
      select! {StToken::OpenParen(_)},
      select! {StToken::ClosedParen(_)},
    )
}

pub fn method_param_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone,
) -> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  select! { StToken::Spread(_) }
    .to(true)
    .or(empty().to(false))
    .then(type_ref_decl_parser)
    .map_with(|(variadic, type_ref), e| {
      MethodParamDecl::new(Box::new(type_ref), variadic).to_ast(range(e))
    })
}

pub fn lambda_decl_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone,
) -> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  // Generic Params
  generic_param_set_parser(type_ref_decl_parser.clone())
    // Method Params
    .then(method_param_set_parser(type_ref_decl_parser.clone()))
    // Arrow
    .then_ignore(select! {StToken::ArrowOp(_)})
    // Return Type
    .then(
      type_ref_decl_parser
        .map(Some)
        .or(select! { StToken::VoidKeyword(_) }.to(None)),
    )
    .map_with(|((generic_params, params), return_type), e| {
      LambdaDecl::new(generic_params, params, return_type.map(|x| Box::new(x))).to_ast(range(e))
    })
}

pub fn unit_parser<'a>(
  type_ref_parser: impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone,
) -> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  type_ref_parser
    .delimited_by(
      select! {StToken::OpenParen(_)},
      select! {StToken::ClosedParen(_)},
    )
    .map_with(|x, e| UnitDecl::new(Box::new(x)).to_ast(range(e)))
}

pub fn sym_ref_decl_parser<'a>(
  type_ref_parser: impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone,
) -> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  // Parse Symbol
  symbol_parser()
    .then(
      type_ref_parser
        .separated_by(select! {StToken::Comma(_)})
        .collect::<Vec<_>>()
        .delimited_by(
          select! {StToken::OpenCaret(_)},
          select! {StToken::ClosedCaret(_)},
        )
        .or_not(),
    )
    .map_with(|(sym, type_ref), e| {
      StAst::new(range(e), TypeRefDecl::new(Box::new(sym), type_ref).into())
    })
}

pub fn type_ref_decl_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  recursive(|type_ref_parser| {
    // Bare symbol
    sym_ref_decl_parser(type_ref_parser.clone())
      // Lambda
      .or(lambda_decl_parser(type_ref_parser.clone()))
      // Unit
      .or(unit_parser(type_ref_parser))
      // Arity
      .then(
        select! {StToken::OpenAngle(_)}
          .then(
            empty()
              .separated_by(select! {StToken::Comma(_)})
              .collect::<Vec<_>>(),
          )
          .then_ignore(select! {StToken::ClosedAngle(_)})
          .map_with(|(_, arity_decl): (_, Vec<()>), e| (arity_decl.iter().count(), range(e)))
          .repeated()
          .collect::<Vec<_>>(),
      )
      // Reduce arity to node
      .map(
        |(type_ref, array_decls): (StAst, Vec<(usize, Range<usize>)>)| {
          array_decls
            .iter()
            .fold(type_ref, |curr, (arity, arity_range)| {
              StAst::new(
                union(arity_range, curr.range()),
                ArrayDecl::new(*arity, Box::new(curr)).into(),
              )
            })
        },
      )
  })
}

pub fn identifier_decl_parser<'a>()
-> impl Parser<'a, &'a [StToken], StAst, extra::Err<Rich<'a, StToken>>> + Clone {
  symbol_parser()
    .then_ignore(select! {StToken::Colon(_)})
    .then(type_ref_decl_parser())
    .map_with(|(sym, type_ref), e| {
      IdentifierDecl::new(Box::new(sym), Box::new(type_ref)).to_ast(range(e))
    })
}

fn range<'a, 'b, E: ParserExtra<'a, &'a [StToken]>>(
  e: &mut MapExtra<'a, 'b, &'a [StToken], E>,
) -> Range<usize> {
  e.span().into_range()
}

fn union(a: &Range<usize>, b: &Range<usize>) -> Range<usize> {
  usize::min(a.start, b.start)..usize::max(a.end, b.end)
}
