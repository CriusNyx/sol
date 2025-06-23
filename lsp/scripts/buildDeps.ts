import { runRustScript, runWithLogs, setScriptOpts } from "./runScripts";
import { program } from "commander";
import * as fs from "fs/promises";

async function generateBarrel() {
  const files = await fs
    .readdir("./sol-types/generated")
    .then((files) => files.filter((file) => file !== "index.ts"));
  const lines = files.map(
    (x) => `export * from "./${x.replace(/\.ts$/, "")}";`
  );
  await fs.writeFile("./sol-types/generated/index.ts", lines.join("\n"));
}

async function buildSolTypes() {
  await runRustScript("cargo test export_bindings");
  await generateBarrel();

  return true;
}

export async function buildDeps() {
  await runWithLogs("Building sol-types", () => buildSolTypes());
  await runWithLogs("Building sol-wasm", () =>
    runRustScript("wasm-pack build --target nodejs --out-dir ../lsp/sol-js")
  );
}

if (require.main === module) {
  program.option("--verbose");

  program.parse();

  const options = program.opts();

  setScriptOpts({ verbose: options.verbose });

  buildDeps().then(() => process.exit());
}
