import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import fs from 'fs'

const isDev = process.env.NODE_ENV !== 'production'

function loadHttpsConfig() {
  if (!isDev) return undefined
  try {
    if (fs.existsSync('./localhost-key.pem') && fs.existsSync('./localhost.pem')) {
      return {
        key: fs.readFileSync('./localhost-key.pem'),
        cert: fs.readFileSync('./localhost.pem'),
      }
    }
  } catch {
    // .pem файлы отсутствуют — работаем на http
  }
  return undefined
}

const httpsConfig = loadHttpsConfig()

export default defineConfig({
  base: '/',
  plugins: [vue(), vueDevTools()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    ...(isDev && {
      ...(httpsConfig
        ? {
            https: httpsConfig,
            hmr: {
              protocol: 'wss',
              host: 'localhost',
            },
          }
        : {}),
    }),
  },
})
