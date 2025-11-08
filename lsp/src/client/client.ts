import * as path from "path";
import { ExtensionContext, window, workspace } from "vscode";
import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
} from "vscode-languageclient/node";
import _ from "lodash";
import "soljs";
import dotnet, { Sol } from "node-api-dotnet";

const Parser = Sol.Parser.SolParser;

let client: LanguageClient | undefined = undefined;

export function activate(context: ExtensionContext) {
  console.log("Starting");

  const serverModule = context.asAbsolutePath(
    path.join("out", "server", "server"),
  );

  const dotnetPath = context.asAbsolutePath(path.join(""));

  const serverOptions: ServerOptions = {
    run: { module: serverModule, transport: TransportKind.ipc },
    debug: { module: serverModule, transport: TransportKind.ipc },
  };

  const clientOptions: LanguageClientOptions = {
    documentSelector: [{ scheme: "file", language: "sol" }],
    synchronize: {
      fileEvents: workspace.createFileSystemWatcher("**/.clientrc"),
    },
  };

  client = new LanguageClient(
    "SolLSP",
    "Sol Language Server",
    serverOptions,
    clientOptions,
  );

  client.start();
}

export async function deactivate() {
  await client?.stop();
}
