import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { pageMetaPlugin } from "./plugins/pageMetaPlugin";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), pageMetaPlugin()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
});
