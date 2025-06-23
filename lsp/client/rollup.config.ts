import { defineConfig } from "rollup";
import resolve from "@rollup/plugin-node-resolve";
import commonJS from "@rollup/plugin-commonjs";
import typescript from "@rollup/plugin-typescript";

export default defineConfig({
  input: "./src/extension.ts",
  output: {
    dir: "./out",
    format: "cjs",
  },
  watch: {
    include: "./src/**",
  },
  plugins: [resolve(), commonJS(), typescript()],
});
