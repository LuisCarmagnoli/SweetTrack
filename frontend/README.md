# SweetTrack: Frontend SPA

Aplicación web cliente (Single Page Application) para la plataforma **SweetTrack**, construida con **Vue 3**, **TypeScript**, **Tailwind CSS** y **Vite**.

Permite a los kiosqueros y clientes registrarse, autenticarse de forma segura mediante JWT, navegar por el catálogo y gestionar sus pedidos con una experiencia visual moderna, responsiva y accesible.

---

## 🧰 Stack Tecnológico

* **Framework:** [Vue 3](https://vuejs.org/) (Composition API con `<script setup lang="ts">`)
* **Build Tool:** [Vite](https://vitejs.dev/)
* **Lenguaje:** [TypeScript](https://www.typescriptlang.org/) (Tipado estricto)
* **Gestor de Estado:** [Pinia](https://pinia.vuejs.org/)
* **Enrutamiento:** [Vue Router](https://router.vuejs.org/)
* **Estilado:** [Tailwind CSS](https://tailwindcss.com/) con soporte nativo de modo oscuro
* **Validación de Formularios:** [Zod](https://zod.dev/) sincronizado con los contratos DTO de .NET
* **Testing:** [Vitest](https://vitest.dev/) y [@vue/test-utils](https://test-utils.vuejs.org/)
* **Cliente HTTP:** Axios con interceptores automáticos de autorización Bearer

---

## 📁 Estructura del Proyecto

```text
frontend/src/
├── assets/         # Estilos globales (main.css) e imágenes
├── components/
│   ├── base/       # Componentes base del Design System (BaseButton, BaseInput, BaseAlert)
│   └── auth/       # Componentes específicos de autenticación (AuthCard)
├── config/         # Configuración de URLs y variables de entorno
├── router/         # Rutas y Guards de navegación (protección de vistas autenticadas)
├── schemas/        # Esquemas de validación con Zod (authSchemas.ts)
├── services/       # Cliente Axios centralizado (apiClient.ts) y servicios API (authService.ts)
├── stores/         # Stores de Pinia (userStore.ts: manejo de token JWT y sesión)
├── types/          # Interfaces y contratos TypeScript espejo del Backend
└── views/
    ├── auth/       # LoginView.vue
    └── dashboard/  # DashboardView.vue
```

---

## 🚀 Guía de Desarrollo

### 1. Instalación de Dependencias
```bash
npm install
```

### 2. Variables de Entorno
Crea o revisa el archivo `.env.development` en la raíz de `frontend/` apuntando a la API de backend:

```env
VITE_API_BASE_URL=http://localhost:5010/api
```

### 3. Iniciar Servidor de Desarrollo
```bash
npm run dev
```
> La aplicación estará disponible en: `http://localhost:5173`

---

## 🧪 Pruebas Unitarias y Calidad de Código

### Ejecutar Suite de Tests (Vitest)
Pruebas de comportamiento sobre componentes, stores de Pinia y esquemas Zod:
```bash
npm run test
```

### Validación Estática de Tipos (TypeScript)
Verifica que no existan errores de compilación de tipos en toda la aplicación:
```bash
npm run type-check
```

### Compilar para Producción
```bash
npm run build
```
Genera los artefactos optimizados en la carpeta `dist/`.
