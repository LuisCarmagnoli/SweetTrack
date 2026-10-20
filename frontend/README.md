# SweetTrack: Frontend SPA

Aplicación web cliente (Single Page Application) para la plataforma **SweetTrack**, desarrollada con **Vue 3**, **TypeScript**, **Tailwind CSS** y **Vite**.

Permite a kiosqueros y administradores registrarse, iniciar sesión con autenticación JWT, consultar catálogos y gestionar pedidos a través de una interfaz responsiva, modular y accesible.

---

## Stack Tecnológico

* **Framework:** [Vue 3](https://vuejs.org/) (Composition API con `<script setup lang="ts">`)
* **Herramienta de Construcción:** [Vite](https://vitejs.dev/)
* **Lenguaje:** [TypeScript](https://www.typescriptlang.org/) (Tipado estricto)
* **Gestión de Estado:** [Pinia](https://pinia.vuejs.org/)
* **Enrutamiento:** [Vue Router](https://router.vuejs.org/)
* **Estilos:** [Tailwind CSS](https://tailwindcss.com/) con soporte para modo oscuro
* **Validación de Formularios:** [Zod](https://zod.dev/) sincronizado con los contratos DTO de .NET
* **Testing:** [Vitest](https://vitest.dev/) y [@vue/test-utils](https://test-utils.vuejs.org/)
* **Cliente HTTP:** Axios con interceptores para inyección de token Bearer

---

## Estructura del Proyecto

```text
frontend/src/
├── assets/         # Estilos globales (main.css) e imágenes
├── components/
│   ├── base/       # Componentes base reutilizables (BaseButton, BaseInput, BaseAlert)
│   └── auth/       # Componentes específicos de autenticación (AuthCard)
├── config/         # Variables de entorno y configuración de endpoints
├── router/         # Rutas y guards de navegación para vistas protegidas
├── schemas/        # Esquemas de validación Zod (authSchemas.ts)
├── services/       # Cliente Axios centralizado (apiClient.ts) y servicios de API (authService.ts)
├── stores/         # Stores de Pinia (userStore.ts: manejo de token JWT y sesión)
├── types/          # Interfaces y tipos TypeScript espejo del backend
└── views/
    ├── auth/       # LoginView.vue
    └── dashboard/  # DashboardView.vue
```

---

## Guía de Desarrollo

### 1. Instalación de Dependencias
```bash
npm install
```

### 2. Variables de Entorno
Configurar el archivo `.env.development` en la raíz de `frontend/` con la URL base de la API:

```env
VITE_API_BASE_URL=http://localhost:5010/api
```

### 3. Iniciar Servidor de Desarrollo
```bash
npm run dev
```
> La aplicación estará disponible en: `http://localhost:5173`

---

## Pruebas Unitarias y Validación

### Ejecutar Suite de Tests (Vitest)
Pruebas de comportamiento sobre componentes, stores de Pinia y esquemas de validación Zod:
```bash
npm run test
```

### Chequeo de Tipos Estáticos (TypeScript)
Valida la integridad de tipos en toda la aplicación:
```bash
npm run type-check
```

### Compilar para Producción
```bash
npm run build
```
Genera los archivos estáticos optimizados en el directorio `dist/`.
