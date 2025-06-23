import { program } from "commander";
import { runScript, runWithLogs, setScriptOpts } from "./runScripts";

export async function build() {
  await runWithLogs("Building server", () =>
    runScript("npx rollup --config rollup.config.ts", "server")
  );
  await runWithLogs("Building client", () =>
    runScript("npx rollup --config rollup.config.ts", "client")
  );
}

if (require.main === module) {
  program.option("--verbose");

  program.parse();

  const options = program.opts();

  setScriptOpts({ verbose: options.verbose });
  build();
}
