import {
  createConnection,
  HandlerResult,
  InitializeParams,
  ProposedFeatures,
  SemanticTokens,
  SemanticTokensBuilder,
  SemanticTokensLegend,
  SemanticTokenTypes,
  TextDocuments,
  TextDocumentSyncKind,
  TokenFormat,
  uinteger,
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import { Sol } from "node-api-dotnet";

import "soljs";

const legend = {
  tokenTypes: Sol.JS.SolSemanticTypes(),
  tokenModifiers: [],
} satisfies SemanticTokensLegend;

const connection = createConnection(ProposedFeatures.all);

const documents = new TextDocuments(TextDocument);

connection.onInitialize((params: InitializeParams) => {
  console.log(connection.onInitialize.name);

  const capabilities = params.capabilities;

  return {
    capabilities: {
      textDocumentSync: TextDocumentSyncKind.Full,
      semanticTokensProvider: {
        full: true,
        legend,
      },
    },
  };
});

connection.onInitialized((params) => {
  console.log(connection.onInitialized.name, params);
});

connection.languages.semanticTokens.on((params) => {
  var doc = documents.get(params.textDocument.uri);
  const docText = doc?.getText() ?? "";
  const tokenJSON = Sol.JS.AnalyzeTokens_JSON(docText);

  console.log(tokenJSON);

  const tokens = JSON.parse(tokenJSON) as Sol.JSSemanticToken[];

  const builder = new SemanticTokensBuilder();

  for (const token of tokens) {
    var pos = doc!.positionAt(token.Start);
    builder.push(
      pos.line,
      pos.character,
      token.Length,
      token.SemanticType,
      0,
    );
  }
  return builder.build();
});

documents.onDidChangeContent((e) => {
  connection.languages.semanticTokens.refresh();
});

documents.listen(connection);

connection.listen();
