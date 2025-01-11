export type {};

await Bun.build({
    root: "./src",
    entrypoints: ["./src/index.ts"],
    format: "esm",
    outdir: "./dist",
    minify: true,
    target: "node",
});
console.log("Completed build");
