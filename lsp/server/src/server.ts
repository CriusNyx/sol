import {
  createConnection,
  HandlerResult,
  Hover,
  InitializeParams,
  MarkedString,
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
import { DevCon } from "node-api-dotnet";
import "devcon-js";

const legend = {
  tokenTypes: DevCon.JS.JSI.DevConSemanticTypes(),
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
      hoverProvider: true,
    },
  };
});

connection.onInitialized((params) => {
  console.log(connection.onInitialized.name, params);
});

connection.languages.semanticTokens.on((params) => {
  var doc = documents.get(params.textDocument.uri);
  const docText = doc?.getText() ?? "";

  const tokenJSON = DevCon.JS.JSI.AnalyzeTokens_JSON(docText);

  console.log(tokenJSON);

  const tokens = JSON.parse(tokenJSON) as DevCon.JS.JSSemanticToken[];

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

connection.onHover((params) => {
  var doc = documents.get(params.textDocument.uri);
  var docText = doc?.getText() ?? "";
  var result = DevCon.JS.JSI.GetElementUnderCursor(
    docText,
    doc?.offsetAt(params.position) ?? -1,
  );
  if (result) {
    return { contents: { kind: "markdown", value: result } } satisfies Hover;
  }
  return undefined;
});

documents.onDidChangeContent((e) => {
  connection.languages.semanticTokens.refresh();
});

documents.listen(connection);

connection.listen();
