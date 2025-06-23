import chalk from "chalk";
import { exec } from "child_process";

interface ScriptOpts {
  verbose?: boolean;
}

let scriptOpts: ScriptOpts | undefined = undefined;

export function setScriptOpts(opts: ScriptOpts) {
  scriptOpts = opts;
}

export async function runScript(script: string, cwd?: string): Promise<void> {
  const [command, ...args] = script.split(" ");
  return new Promise((res, err) => {
    const child = exec(script, {
      cwd,
    });
    child.on("exit", () => {
      if (child.exitCode === 0) {
        res();
      } else {
        err(`process existed with code ${child.exitCode}`);
      }
    });
  });
}

export async function runRustScript(script: string) {
  return await runScript(script, "../sol");
}

export async function runWithLogs(title: string, func: () => Promise<any>) {
  console.log(chalk.bold.green(title));
  await func()
    .then(() => console.log(chalk.bold.green("Success")))
    .catch((e) => {
      console.error(e);
      throw e;
    });
}
