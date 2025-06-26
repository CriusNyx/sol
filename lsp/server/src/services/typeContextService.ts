import { Connection, TextDocuments } from "vscode-languageserver/node";
import { Service } from "./service";
import { TextDocument } from "vscode-languageserver-textdocument";
import { create_type_system_context } from "../../../sol-js/sol";
import * as fs from "fs/promises";
import * as fsSync from "fs";
import * as path from "path";
import * as url from "url";

const typeContext = create_type_system_context();

export type TypeContextService = ReturnType<typeof createTypeContextService>;

async function loadWorkspace(uris: string[]) {
  const folderPaths = uris.map((uri) => url.fileURLToPath(uri));

  return await Promise.all(folderPaths.map(visitFolder));
}

async function visitFolder(dirPath: string) {
  const entries = await fs.readdir(dirPath);

  return await Promise.all(
    entries.map(async (entry) => {
      const entryPath = path.join(dirPath, entry);
      try {
        const stats = await fs.lstat(entryPath);
        if (stats.isFile()) {
          if (entryPath.endsWith(".st")) {
            await visitSTFile(entryPath);
          }
        } else {
          await visitFolder(entryPath);
        }
      } finally {
      }
    })
  );
}

async function visitSTFile(path: string) {
  typeContext.new_doc(path);
  const source = fsSync.readFileSync(path, "utf8");
  typeContext.update_doc_text(path, source);
}

export function createTypeContextService(
  connection: Connection,
  documents: TextDocuments<TextDocument>
) {
  return {
    onInitialize(params) {
      loadWorkspace(params.workspaceFolders.map((x) => x.uri));
    },
    destroy() {
      typeContext.free();
    },
  } satisfies Service;
}
