import { Position, TextDocument } from "vscode-languageserver-textdocument";
import { Service } from "./service";
import { analyzeSemantics, SemanticType, semanticTypes } from "../semantics";
import { Connection, TextDocuments } from "vscode-languageserver/node";

export type SemanticsService = ReturnType<typeof createSemanticService>;

export function createSemanticService(
  connection: Connection,
  documents: TextDocuments<TextDocument>
) {
  connection.languages.semanticTokens.on((params) => {
    const doc = documents.get(params.textDocument.uri);

    if (!doc) {
      return { data: [] };
    }

    const docText = doc.getText();

    const sourceTokens = analyzeSemantics(docText).map((x) => ({
      position: x.start,
      length: x.len,
      type: x.semanticType,
      modifier: 0,
    }));

    let previousPosition: Position = { line: 0, character: 0 };
    let previousToken: (typeof sourceTokens)[number] | undefined = undefined;

    const tokens: number[] = [];
    for (const token of sourceTokens) {
      const position = doc.positionAt(token.position);
      if (previousToken === undefined) {
        tokens.push(
          ...encodeToken(
            position.line,
            position.character,
            token.length,
            token.type,
            token.modifier
          )
        );
      } else {
        tokens.push(
          ...encodeToken(
            position.line - previousPosition.line,
            position.line === previousPosition.line
              ? position.character - previousPosition.character
              : position.character,
            token.length,
            token.type,
            token.modifier
          )
        );
      }
      previousPosition = position;
      previousToken = token;
    }
    return { data: tokens };
  });
  return {} satisfies Service;
}

function encodeToken(
  line: number,
  char: number,
  length: number,
  tokenType: SemanticType,
  tokenModifiers: number
): number[] {
  return [line, char, length, semanticTypes.indexOf(tokenType), tokenModifiers];
}
