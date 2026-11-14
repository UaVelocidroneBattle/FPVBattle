import { defineConfig } from "@hey-api/openapi-ts";
import { defaultPlugins } from "@hey-api/openapi-ts";

export default defineConfig({
  client: "@hey-api/client-fetch",
  input: "../shared/api/FpvBattle.Web.json",
  output: "src/api/client",
  experimentalParser: true,
  plugins: [
    ...defaultPlugins,
    // {
    //   name: "@hey-api/sdk",
    //   transformer: true,
    // },
    // {
    //   name: "@hey-api/transformers",
    //   dates: true,
    // },
  ],
});
