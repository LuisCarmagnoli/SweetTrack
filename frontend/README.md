# SweetTrack: Frontend SPA (Vue 3, TypeScript & Tailwind CSS)

Aplicación web cliente (Single Page Application) para la plataforma comercial B2B **SweetTrack**, diseñada para kioscos, almacenes y operarios de depósito mayoristas. Desarrollada con **Vue 3** (Composition API), **TypeScript** en modo estricto, **Tailwind CSS**, **Pinia** y **Vite**.

Permite gestionar el catálogo mayorista, armar carritos con control de umbral mínimo ($50.000 ARS), confirmar pedidos mediante checkout validado con Zod, seguir estados de entrega en tiempo real vía WebSockets (SignalR) y administrar el despacho logístico con descarga de remitos comerciales en PDF.

---

## 🛠️ Stack Tecnológico

* **Framework:** [Vue 3](https://vuejs.org/) (Composition API con `<script setup lang="ts">`)
* **Herramienta de Construcción:** [Vite](https://vitejs.dev/)
* **Lenguaje:** [TypeScript](https://www.typescriptlang.org/) (Tipado estricto sin tipos `any`)
* **Gestión de Estado:** [Pinia](https://pinia.vuejs.org/) (Stores modulares y tipados)
* **Enrutamiento:** [Vue Router 4](https://router.vuejs.org/) con navegación basada en roles
* **Estilos & Diseño:** [Tailwind CSS](https://tailwindcss.com/) con diseño adaptativo y modo oscuro
* **Validación de Formularios:** [Zod](https://zod.dev/) sincronizado con los contratos DTO del backend
* **Comunicación en Tiempo Real:** [@microsoft/signalr](https://www.npmjs.com/package/@microsoft/signalr) con ticket efímero
* **Cliente HTTP:** [Axios](https://axios-http.com/) con interceptores para inyección y renovación de tokens JWT
* **Testing:** [Vitest](https://vitest.dev/) y [@vue/test-utils](https://test-utils.vuejs.org/) (55 pruebas unitarias)

---

## 📂 Estructura del Proyecto

```text
frontend/
├── src/
│   ├── assets/              # Estilos globales (main.css), imágenes e isotipos
│   ├── components/
│   │   ├── auth/            # AuthCard.vue (tarjeta contenedora de login/registro)
│   │   ├── base/            # Componentes atómicos (BaseButton, BaseInput, BaseAlert, BaseBadge, BaseModal)
│   │   ├── cart/            # CartDrawer.vue (carrito lateral), CheckoutModal.vue (confirmación de compra)
│   │   └── catalog/         # ProductCard.vue, CatalogFilters.vue, BulkImportModal.vue, StockAdjustmentModal.vue
│   ├── config/              # api.ts (endpoints base y constantes globales)
│   ├── router/              # index.ts (rutas, metadatos y guards: requiresAuth, adminOnly, guestOnly)
│   ├── schemas/             # Esquemas de validación Zod (authSchemas.ts, catalogSchemas.ts, orderingSchemas.ts)
│   ├── services/            # Clientes de API: apiClient.ts, authService.ts, catalogService.ts, orderingService.ts, signalrService.ts
│   ├── stores/              # Stores Pinia: userStore.ts, catalogStore.ts, cartStore.ts, orderStore.ts
│   ├── types/               # Modelos e interfaces TypeScript espejo de contratos C# (auth.ts, catalog.ts, ordering.ts)
│   ├── views/
│   │   ├── auth/            # LoginView.vue (inicio de sesión y registro de kioscos)
│   │   ├── catalog/         # CatalogView.vue (catálogo de golosinas con filtros y búsqueda)
│   │   ├── dashboard/       # DashboardView.vue (panel de bienvenida y accesos rápidos)
│   │   ├── dispatch/        # DispatchBoardView.vue (tablero Kanban de despacho para operarios)
│   │   └── orders/          # MyOrdersView.vue (historial y seguimiento en vivo con stepper)
│   ├── App.vue              # Componente raíz con Navbar reactivo y soporte de modales
│   └── main.ts              # Punto de entrada, registro de Pinia y Router
├── tests/
│   └── unit/                # 9 suites de pruebas unitarias con Vitest (55 tests)
│       ├── authSchemas.spec.ts
│       ├── cartStore.spec.ts
│       ├── CatalogFilters.spec.ts
│       ├── catalogStore.spec.ts
│       ├── LoginView.spec.ts
│       ├── orderingSchemas.spec.ts
│       ├── orderStore.spec.ts
│       ├── ProductCard.spec.ts
│       └── userStore.spec.ts
├── vite.config.ts           # Configuración de Vite y proxy inverso para desarrollo
├── tsconfig.json            # Configuración estricta de TypeScript
└── package.json             # Dependencias y scripts de ejecución
```

---

## 🌟 Módulos y Funcionalidades Principales

### 1. Autenticación y Control de Acceso (`/login`)
- Flujo unificado de inicio de sesión y registro rápido para kioscos.
- Persistencia del token JWT en `localStorage` con expiración e inyección en cabeceras `Authorization: Bearer <token>`.
- Rutas protegidas mediante `router.beforeEach`:
  - `requiresAuth`: Requiere sesión activa.
  - `adminOnly`: Restringido exclusivamente al rol `Admin`.
  - `guestOnly`: Solo accesible si el usuario no tiene sesión iniciada.

### 2. Catálogo Mayorista B2B (`/catalog`)
- Exploración de golosinas agrupadas por marca (Arcor, Mondelez, Felfort, etc.) y categoría (Chocolates, Caramelos, Galletitas, etc.).
- Búsqueda reactiva por texto en tiempo real.
- Visualización de precios por bulto, unidades contenidas y stock disponible con badges de alerta.
- **Acciones Rápidas:** Botón "+1 Bulto al Carrito" con badge interactivo de cantidad acumulada.
- **Herramientas de Administrador:**
  - Modal de ajuste de inventario manual con motivo de auditoría Kardex.
  - Modal de importación masiva de planillas Excel (`.xlsx`) arrastrando y soltando archivos.

### 3. Carrito Mayorista y Checkout (`CartDrawer` & `CheckoutModal`)
- **`cartStore.ts`:** Gestión de compras mayoristas con persistencia local.
- **Cálculo Fiscal Automático:** Desglose reactivo de Subtotal, 21% de IVA y Total en ARS.
- **Barra de Progreso de Umbral:** Indicador visual dinámico hacia el mínimo de compra mayorista ($50.000 ARS). El botón de checkout se habilita únicamente al alcanzar o superar el umbral.
- **Modal de Checkout:** Formulario validado con `Zod` (`checkoutOrderSchema`) para capturar dirección de entrega, notas para el repartidor y selección de método de pago (Efectivo contra entrega, Transferencia bancaria, Cuenta corriente).

### 4. Seguimiento en Tiempo Real de Pedidos (`/my-orders`)
- Historial completo de pedidos del kiosco autenticado.
- **Stepper Visual de 5 Etapas:** Indicador gráfico en vivo del avance (`Pendiente de Confirmación` ➔ `En Preparación` ➔ `Listo para Despacho` ➔ `En Reparto` ➔ `Entregado`).
- **Push en Tiempo Real:** Integración con SignalR (`orderStore.ts`). Al conectarse, el cliente se une a su grupo privado `kiosk-{userId}` y recibe eventos `OrderStatusChanged` en vivo sin necesidad de recargar la página.
- **Cancelación Segura:** Opción de cancelar la orden en un clic únicamente mientras se encuentre en estado `Pendiente de Confirmación`.

### 5. Tablero Kanban de Despacho (`/dispatch`)
- Vista operativa reservada para Administradores y Operarios de Depósito.
- **Columnas de Flujo:** `En Preparación`, `Listo para Despacho` y `En Reparto`.
- **Transición con un Clic:** Botones directos para avanzar las órdenes al siguiente estado operativo.
- **Generación y Descarga de Remitos:** Botón "Descargar Remito (PDF)" que realiza una petición autenticada al endpoint `/api/dispatch/orders/{id}/remito-pdf`, recibiendo el stream binario y descargando automáticamente el archivo PDF oficial.

---

## 🔄 Proxy Inverso en Desarrollo (`vite.config.ts`)

Para evitar problemas de CORS y simplificar la conexión local a los tres microservicios de backend, el servidor de desarrollo de Vite redirige el tráfico de forma transparente:

```text
Frontend (Puerto 5173)
├── /api/auth/*     ──> http://localhost:5010 (SweetTrack-AuthAPI)
├── /api/catalog/*  ──> http://localhost:5020 (SweetTrack-CatalogAPI)
├── /api/orders/*   ──> http://localhost:5030 (SweetTrack-OrderingAPI)
├── /api/dispatch/* ──> http://localhost:5030 (SweetTrack-OrderingAPI)
└── /hubs/orders    ──> ws://localhost:5030  (WebSockets SignalR Hub)
```

---

## 🚀 Guía de Desarrollo Local

### 1. Prerrequisitos
* [Node.js](https://nodejs.org/) (versión 20 o superior recomendada)
* `npm` (versión 10 o superior)

### 2. Instalación de Dependencias
```bash
cd frontend
npm install
```

### 3. Iniciar el Servidor de Desarrollo
```bash
npm run dev
```
La aplicación estará disponible en: **[http://localhost:5173](http://localhost:5173)**.

---

## 🧪 Pruebas Unitarias y Calidad de Código

### Ejecutar Suite de Tests (Vitest)
Ejecuta las 55 pruebas unitarias cubriendo componentes, stores de Pinia, servicios y validaciones Zod:
```bash
npm test
```

Para ejecutar una única corrida sin modo interactivo:
```bash
npm test -- --run
```

### Verificación Estricta de Tipos (TypeScript)
Valida la consistencia de tipos en todos los archivos `.ts` y componentes `.vue`:
```bash
npm run type-check
```

### Compilar para Producción
Compila y optimiza los artefactos finales para despliegue (en Azure Static Web Apps u otro hosting estático):
```bash
npm run build
```
Los archivos de distribución generados se almacenarán en la carpeta `frontend/dist/`.

---

## 👤 Credenciales de Prueba en Desarrollo

* **Administrador / Operador de Depósito:**
  * **Email:** `admin@sweettrack.com`
  * **Password:** `Admin123*!`
  * **Acceso:** Gestión de stock, carga masiva Excel y Tablero de Despacho Kanban (`/dispatch`).
* **Kiosco / Cliente B2B:**
  * Puede registrarse directamente desde la pantalla de login con cualquier correo y contraseña válida para armar carritos, emitir órdenes y probar el seguimiento en tiempo real.
