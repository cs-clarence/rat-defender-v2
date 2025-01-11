#!/usr/bin/env bun
import stripBom from "strip-bom";
import pkgInfo from "../package.json";
import fs from "node:fs/promises";
import jschardet from "jschardet";
import { Command } from "@commander-js/extra-typings";

const program = new Command();

async function strip(fileOrDirs: string[]) {
    for (const f of fileOrDirs) {
        if (!(await fs.exists(f))) {
            console.error(`Path does not exist: ${f}`);
            process.exit(1);
        }
    }

    for (const fileOrDir of fileOrDirs) {
        if ((await fs.stat(fileOrDir)).isDirectory()) {
            const contents = (await fs.readdir(fileOrDir)).map(
                (c) => `${fileOrDir}/${c}`,
            );

            await strip(contents);
        } else {
            const contents = await fs.readFile(fileOrDir);

            if (jschardet.detect(contents).encoding === "UTF-8") {
                const utf8Content = contents.toString("utf8");
                await fs.writeFile(fileOrDir, stripBom(utf8Content));
            }
        }
    }
}

program
    .name("strip-utf8-bom")
    .description("Strips BOM from UTF-8 encoded files")
    .version(pkgInfo.version)
    .argument("<fileOrDir...>", "Files to strip BOM from")
    .action(strip);

await program.parseAsync();
