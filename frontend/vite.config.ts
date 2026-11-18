import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ isSsrBuild }) => ({
  plugins: [react()],
  // `vite build --ssr src/entry-server.tsx` bundles the app (incl. React) into one
  // self-contained script that the .NET backend runs to prerender the page.
  ...(isSsrBuild && {
    ssr: { target: 'webworker' as const, noExternal: true },
    define: { 'process.env.NODE_ENV': JSON.stringify('production') },
    build: {
      outDir: 'dist-ssr',
      emptyOutDir: true,
      copyPublicDir: false,
      rollupOptions: {
        output: { format: 'iife' as const, name: 'CarltonSSR', entryFileNames: 'entry-server.js', inlineDynamicImports: true },
      },
    },
  }),
}))
