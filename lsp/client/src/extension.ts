/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */

import * as path from "path";
import { workspace, ExtensionContext, Uri } from "vscode";

import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
} from "vscode-languageclient/node";

let client: LanguageClient;

export function activate(context: ExtensionContext) {
  // The server is implemented in node
  const serverModule = context.asAbsolutePath(
    path.join("server", "out", "server.js")
  );

  // If the extension is launched in debug mode then the debug server options are used
  // Otherwise the run options are used
  const serverOptions: ServerOptions = {
    run: { module: serverModule, transport: TransportKind.ipc },
    debug: {
      module: serverModule,
      transport: TransportKind.ipc,
    },
  };

  // Options to control the language client
  const clientOptions: LanguageClientOptions = {
    // Register the server for plain text documents
    documentSelector: [
      { scheme: "file", language: "sol_type", pattern: "**/*.st" },
    ],
    synchronize: {
      // Notify the server about file changes to '.st files contained in the workspace
      fileEvents: workspace.createFileSystemWatcher("**/*.st"),
    },
    uriConverters: {
      code2Protocol: (uri) => uri.toString(true),
      protocol2Code: (value) => Uri.parse(value),
    },
  };

  // Create the language client and start the client.
  client = new LanguageClient(
    "SolLanguageServer",
    "Sol Language Server",
    serverOptions,
    clientOptions
  );

  // Start the client. This will also launch the server
  client.start();
}

export function deactivate(): Promise<void> | undefined {
  if (!client) {
    return undefined;
  }
  return client.stop();
}
