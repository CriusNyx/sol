use std::ops::Range;

use chumsky::{extra::ParserExtra, input::MapExtra, prelude::*};

use crate::type_program::{
  nodes::{
    array_decl::ArrayDecl,
    ast_node::{ASTNode, ToAST},
    field_decl::FieldDecl,
    generic_param_decl::GenericParamDecl,
    global_decl::GlobalDecl,
    identifier::IdentifierDecl,
    lambda_decl::LambdaDecl,
    method_decl::MethodDecl,
    method_param_decl::MethodParamDecl,
    symbol_node::SymbolNode,
    type_decl::TypeDecl,
    type_program_node::TypeProgramNode,
    type_ref_decl::TypeRefDecl,
    unit_decl::UnitDecl,
  },
  type_token::TypeToken,
};

pub fn symbol_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  select! {TypeToken::Symbol(info) => TypeToken::Symbol(info)}
    .map_with(|x, e| SymbolNode::new(x.get_info().source.to_string()).to_ast(range(e)))
}

pub fn type_program_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  top_level_statement_parser()
    .repeated()
    .collect::<Vec<_>>()
    .map_with(|x, e| TypeProgramNode::new(x).to_ast(range(e)))
}

pub fn top_level_statement_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  choice((global_decl_parser(), type_decl_parser()))
}

pub fn global_decl_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  select! {TypeToken::StaticKeyword(_)}
    .then(identifier_decl_parser())
    .then_ignore(select! {TypeToken::Semicolon(_)})
    .map_with(|(_, ident), e| GlobalDecl::new(Box::new(ident)).to_ast(range(e)))
}

pub fn field_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  select! {TypeToken::StaticKeyword(_)}
    .to(true)
    .or(empty().to(false))
    .then(identifier_decl_parser())
    .then_ignore(select! {TypeToken::Semicolon(_)})
    .map_with(|(is_static, identifier), e| {
      FieldDecl::new(Box::new(identifier), is_static).to_ast(range(e))
    })
}

pub fn method_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  // Static keyword
  select! {TypeToken::StaticKeyword(_)}
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
      select! {TypeToken::Colon(_)}
        .then(type_ref_decl_parser())
        .map(|(_, x)| Some(Box::new(x))),
      select! {TypeToken::Colon(_)}
        .then_ignore(select! {TypeToken::VoidKeyword(_)})
        .to(None),
      empty().to(None),
    )))
    .then_ignore(select! {TypeToken::Semicolon(_)})
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
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  choice((field_parser(), method_parser()))
}

pub fn type_decl_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  // Type Keyword
  select! {TypeToken::TypeKeyword(_)}
    // Symbol Parser
    .then(symbol_parser())
    // Generic params
    .then(generic_param_set_parser(type_ref_decl_parser()))
    // Inherits
    .then(
      select! {TypeToken::Colon(_)}
        .then(
          type_ref_decl_parser()
            .separated_by(select! {TypeToken::AddOp(_)})
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
          select! { TypeToken::OpenCurly(_) },
          select! { TypeToken::ClosedCurly(_)},
        )
        .map(Some)
        .or(select! {TypeToken::Semicolon(_)}.to(None)),
    )
    .map_with(|((((_, sym), generic_param), inherits), body), e| {
      TypeDecl::new(Box::new(sym), generic_param, inherits, body).to_ast(range(e))
    })
}

pub fn generic_param_set_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>>
  + Clone,
) -> impl Parser<'a, &'a [TypeToken], Option<Vec<ASTNode>>, extra::Err<Rich<'a, TypeToken>>> + Clone
{
  generic_param_parser(type_ref_decl_parser)
    .separated_by(select! {TypeToken::Comma(_)})
    .collect::<Vec<_>>()
    .delimited_by(
      select! {TypeToken::OpenCaret(_)},
      select! {TypeToken::ClosedCaret(_)},
    )
    .or_not()
}

pub fn generic_param_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>>
  + Clone,
) -> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  symbol_parser()
    .then(
      select! {TypeToken::Colon(_)}
        .then(
          type_ref_decl_parser
            .separated_by(select! {TypeToken::AddOp(_)})
            .collect::<Vec<_>>(),
        )
        .map(|(_, inherits)| inherits)
        .or_not(),
    )
    .map_with(|(sym, inherits), e| GenericParamDecl::new(Box::new(sym), inherits).to_ast(range(e)))
}

pub fn method_param_set_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>>
  + Clone,
) -> impl Parser<'a, &'a [TypeToken], Vec<ASTNode>, extra::Err<Rich<'a, TypeToken>>> + Clone {
  method_param_parser(type_ref_decl_parser)
    .separated_by(select! {TypeToken::Comma(_)})
    .collect::<Vec<_>>()
    .delimited_by(
      select! {TypeToken::OpenParen(_)},
      select! {TypeToken::ClosedParen(_)},
    )
}

pub fn method_param_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>>
  + Clone,
) -> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  select! { TypeToken::Spread(_) }
    .to(true)
    .or(empty().to(false))
    .then(type_ref_decl_parser)
    .map_with(|(variadic, type_ref), e| {
      MethodParamDecl::new(Box::new(type_ref), variadic).to_ast(range(e))
    })
}

pub fn lambda_decl_parser<'a>(
  type_ref_decl_parser: impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>>
  + Clone,
) -> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  // Generic Params
  generic_param_set_parser(type_ref_decl_parser.clone())
    // Method Params
    .then(method_param_set_parser(type_ref_decl_parser.clone()))
    // Arrow
    .then_ignore(select! {TypeToken::ArrowOp(_)})
    // Return Type
    .then(
      type_ref_decl_parser
        .map(Some)
        .or(select! { TypeToken::VoidKeyword(_) }.to(None)),
    )
    .map_with(|((generic_params, params), return_type), e| {
      LambdaDecl::new(generic_params, params, return_type.map(|x| Box::new(x))).to_ast(range(e))
    })
}

pub fn unit_parser<'a>(
  type_ref_parser: impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone,
) -> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  type_ref_parser
    .delimited_by(
      select! {TypeToken::OpenParen(_)},
      select! {TypeToken::ClosedParen(_)},
    )
    .map_with(|x, e| UnitDecl::new(Box::new(x)).to_ast(range(e)))
}

pub fn sym_ref_decl_parser<'a>(
  type_ref_parser: impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone,
) -> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  // Parse Symbol
  symbol_parser()
    .then(
      type_ref_parser
        .separated_by(select! {TypeToken::Comma(_)})
        .collect::<Vec<_>>()
        .delimited_by(
          select! {TypeToken::OpenCaret(_)},
          select! {TypeToken::ClosedCaret(_)},
        )
        .or_not(),
    )
    .map_with(|(sym, type_ref), e| {
      ASTNode::new(range(e), TypeRefDecl::new(Box::new(sym), type_ref).into())
    })
}

pub fn type_ref_decl_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  recursive(|type_ref_parser| {
    // Bare symbol
    sym_ref_decl_parser(type_ref_parser.clone())
      // Lambda
      .or(lambda_decl_parser(type_ref_parser.clone()))
      // Unit
      .or(unit_parser(type_ref_parser))
      // Arity
      .then(
        select! {TypeToken::OpenAngle(_)}
          .then(
            empty()
              .separated_by(select! {TypeToken::Comma(_)})
              .collect::<Vec<_>>(),
          )
          .then_ignore(select! {TypeToken::ClosedAngle(_)})
          .map_with(|(_, arity_decl): (_, Vec<()>), e| (arity_decl.iter().count(), range(e)))
          .repeated()
          .collect::<Vec<_>>(),
      )
      // Reduce arity to node
      .map(
        |(type_ref, array_decls): (ASTNode, Vec<(usize, Range<usize>)>)| {
          array_decls
            .iter()
            .fold(type_ref, |curr, (arity, arity_range)| {
              ASTNode::new(
                union(arity_range, curr.range()),
                ArrayDecl::new(*arity, Box::new(curr)).into(),
              )
            })
        },
      )
  })
}

pub fn identifier_decl_parser<'a>()
-> impl Parser<'a, &'a [TypeToken], ASTNode, extra::Err<Rich<'a, TypeToken>>> + Clone {
  symbol_parser()
    .then_ignore(select! {TypeToken::Colon(_)})
    .then(type_ref_decl_parser())
    .map_with(|(sym, type_ref), e| {
      IdentifierDecl::new(Box::new(sym), Box::new(type_ref)).to_ast(range(e))
    })
}

fn range<'a, 'b, E: ParserExtra<'a, &'a [TypeToken]>>(
  e: &mut MapExtra<'a, 'b, &'a [TypeToken], E>,
) -> Range<usize> {
  e.span().into_range()
}

fn union(a: &Range<usize>, b: &Range<usize>) -> Range<usize> {
  usize::min(a.start, b.start)..usize::max(a.end, b.end)
}
