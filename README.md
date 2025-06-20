# Sol

Work in progress.

Context sensitive programming language designed to be integrated as an
interactive console.

Goal is to create a language which uses types to infer parameter binding,
allowing for the omission of a lot of the punctuation found in most block scoped
languages. IE: eliminate as much as possible, commas, parens, and semicolons.

- [x] Create parser for type system
- [ ] Refactor parser to use shared references instead of cloning everything
- [ ] Add lexons to parser using shared references
- [ ] Create type system interpreter
- [ ] Create lsp for st files.
