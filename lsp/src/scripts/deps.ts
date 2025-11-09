import { program } from "@commander-js/extra-typings";
import { exec, execSync, spawn, spawnSync } from "node:child_process";

var parser = program.option("--release", "Set configuration to release");

type ProgramOpts = Parameters<Parameters<typeof parser.action>[0]>[0];

function command(commandString: string) {
  execSync(commandString, { stdio: "inherit" });
}

function buildDotNet(opts: ProgramOpts) {
  let args = "";
  if (opts.release) {
    args += " --configuration Release";
  }
  command(`(cd ../core; dotnet build${args})`);
}

function install(opts: ProgramOpts) {
  command("rm -rf ./server/node_modules/devcon-js");
  command("(cd server; yarn install --check-files)");
}

function buildDeps(opts: ProgramOpts) {
  buildDotNet(opts);
  install(opts);
}

parser.action(buildDeps).parse();
