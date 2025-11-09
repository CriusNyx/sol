import { execSync } from "node:child_process";
import { version } from "../../package.json";

function command(commandString: string) {
  execSync(commandString, { stdio: "inherit" });
}

function run() {
  command("vsce package");
  command(`code --install-extension devcon-lsp-${version}.vsix`);
  command(`rm devcon-lsp-${version}.vsix`);
}

run();
