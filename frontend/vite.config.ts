import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath, URL } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/hubs/orders': {
        target: 'http://localhost:5030',
        changeOrigin: true,
        ws: true,
      },
      '/api/orders': {
        target: 'http://localhost:5030',
        changeOrigin: true,
        secure: false,
      },
      '/api/dispatch': {
        target: 'http://localhost:5030',
        changeOrigin: true,
        secure: false,
      },
      '/api/catalog': {
        target: 'http://localhost:5020',
        changeOrigin: true,
        secure: false,
      },
      '/api': {
        target: 'http://localhost:5010',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
