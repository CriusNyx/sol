// build.ts
import esbuild, { Plugin } from "esbuild";
import { wasmLoader } from "esbuild-plugin-wasm";
import fs from "fs";
import path from "path";

esbuild
  .build({
    entryPoints: ["./src/server.ts"],
    bundle: true,
    outfile: "./out/main.js",
    external: ["vscode"],
    format: "esm",
    platform: "node",
    plugins: [
      copyWasmPlugin({
        from: "node_modules/sol/sol_bg.wasm",
        to: "out/sol_bg.wasm",
      }),
    ],
  })
  .then(() => console.log("done"));

function copyWasmPlugin(options: { from: string; to: string }): Plugin {
  return {
    name: "copy-wasm",
    setup(build) {
      build.onEnd(() => {
        const fromPath = path.resolve(options.from);
        const toPath = path.resolve(
          build.initialOptions.outdir || ".",
          options.to
        );

        fs.mkdirSync(path.dirname(toPath), { recursive: true });
        fs.copyFileSync(fromPath, toPath);

        console.log(`[copy-wasm] Copied ${fromPath} -> ${toPath}`);
      });
    },
  };
}
