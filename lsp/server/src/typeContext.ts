import { _Connection, TextDocuments } from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import { create_type_system_context } from "../../sol-js/sol";

const internalTypeContext = create_type_system_context();

function registerEventHandlers(
  connection: _Connection,
  documents: TextDocuments<TextDocument>
) {
  console.log("registering event handlers");
  connection.onDidChangeWatchedFiles((e) => {
    console.log("did change");
    console.log({ watchedFilesEvent: e });
  });
}

export const TypeContext = {
  registerEventHandlers,
} as const;
