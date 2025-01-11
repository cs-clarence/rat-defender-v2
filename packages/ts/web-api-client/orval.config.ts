import { defineConfig } from "orval";

export default defineConfig({
    gateway: {
        input: {
            target: `${process.env.API_BASE_URL ?? "http://localhost:5149"}/openapi/v1.json`,
        },
        output: {
            indexFiles: true,
            client: "fetch",
            clean: true,
            mode: "tags",
            biome: true,
            workspace: "./src",
            target: "./gen/endpoints",
            schemas: "./gen/schemas",
            headers: true,
            override: {
                mutator: {
                    path: "./mutator/custom-fetch.ts",
                    name: "customFetch",
                },
            },
            // mock: {
            //     type: "msw",
            // },
        },
    },
});
