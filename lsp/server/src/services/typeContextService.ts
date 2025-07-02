import {
  Connection,
  FileChangeType,
  TextDocuments,
} from "vscode-languageserver/node";
import { Service } from "./service";
import { TextDocument } from "vscode-languageserver-textdocument";
import { create_type_context, TypeSystemContext } from "../../../sol-js/sol";
import { createFuture } from "../util/future";
import { using } from "../util/using";
import * as fs from "fs/promises";
import * as fsSync from "fs";
import * as path from "path";
import * as url from "url";

export type TypeContextService = ReturnType<typeof createTypeContextService>;

async function initializeTypeContext(uris: string[]) {
  const typeContext = create_type_context();

  const folderPaths = uris.map((uri) => url.fileURLToPath(uri));

  await Promise.all(folderPaths.map((path) => visitFolder(path, typeContext)));

  return typeContext;
}

async function visitFolder(dirPath: string, typeContext: TypeSystemContext) {
  const entries = await fs.readdir(dirPath);

  return await Promise.all(
    entries.map(async (entry) => {
      const entryPath = path.join(dirPath, entry);

      const stats = await fs.lstat(entryPath);
      if (stats.isFile()) {
        if (entryPath.endsWith(".st")) {
          await visitSTFile(entryPath, typeContext);
        }
      } else {
        await visitFolder(entryPath, typeContext);
      }
    })
  );
}

function updateDocSource(
  context: TypeSystemContext,
  uri: string,
  source: string
) {
  using(context.borrow(uri), (x) => x.set_source(source));
}

async function visitSTFile(filepath: string, typeContext: TypeSystemContext) {
  const docIdent = url.pathToFileURL(filepath).toString();
  typeContext.new_doc(docIdent);
  const source = fsSync.readFileSync(filepath, "utf8");
  updateDocSource(typeContext, docIdent, source);
}

export function createTypeContextService(
  connection: Connection,
  documents: TextDocuments<TextDocument>
) {
  let [typeContext, setTypeContext] = createFuture<TypeSystemContext>();

  connection.onDidChangeWatchedFiles(async (e) => {
    const context = await typeContext;
    for (const change of e.changes) {
      const docText = documents.get(change.uri).getText();
      switch (change.type) {
        case FileChangeType.Created:
          context.new_doc(change.uri);
          updateDocSource(context, change.uri, docText);
          break;
        case FileChangeType.Changed:
          updateDocSource(context, change.uri, docText);
          break;
        case FileChangeType.Deleted:
          context.remove_doc(change.uri);
          break;
      }
    }
    const docs = context.get_doc_identifiers();

    console.log(docs);
  });

  return {
    onInitialize(params) {
      initializeTypeContext(params.workspaceFolders.map((x) => x.uri)).then(
        setTypeContext
      );
    },
    destroy() {
      typeContext.then((x) => x.free);
    },
  } satisfies Service;
}
