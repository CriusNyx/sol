import { build } from "./build";
import { buildDeps } from "./buildDeps";
import { runScript, runWithLogs } from "./runScripts";

export async function installPackage() {
  await buildDeps();
  await build();
  await runWithLogs("Packaging", () => runScript("vsce package"));
  await runWithLogs("Installing", () =>
    runScript("code --install-extension sol-language-server-1.0.0.vsix")
  );
}

if (require.main === module) {
  installPackage();
}
