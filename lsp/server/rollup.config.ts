import { defineConfig } from "rollup";
import resolve from "@rollup/plugin-node-resolve";
import commonJS from "@rollup/plugin-commonjs";
import typescript from "@rollup/plugin-typescript";

export default defineConfig({
  input: "./src/server.ts",
  external: [/.*\/sol-js\/sol/, /.*\/sol-js\/sol\/.*/],
  output: {
    dir: "./out",
    format: "cjs",
  },
  watch: {
    include: "./src/**",
  },
  plugins: [
    resolve(),
    commonJS(),
    typescript({ allowImportingTsExtensions: true }),
  ],
});
