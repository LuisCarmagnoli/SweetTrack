<script setup lang="ts">
import { useRouter } from 'vue-router'
import {
  Store,
  LogOut,
  Package,
  ShoppingCart,
  ShieldCheck,
  TrendingUp,
  User,
  Key,
  Truck,
} from '@lucide/vue'
import { useUserStore } from '@/stores/userStore'
import BaseButton from '@/components/base/BaseButton.vue'

const router = useRouter()
const userStore = useUserStore()

function handleLogout() {
  userStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col">
    <!-- Navbar Superior -->
    <header class="border-b border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900/90 backdrop-blur-md sticky top-0 z-20">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <!-- Logo SweetTrack -->
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-tr from-indigo-600 to-amber-500 flex items-center justify-center text-white shadow-md shadow-indigo-500/20">
            <Store class="w-5 h-5" />
          </div>
          <div>
            <span class="font-extrabold text-lg tracking-tight">SweetTrack</span>
            <span class="ml-2 text-xs font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-indigo-50 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-400 border border-indigo-200 dark:border-indigo-800">
              B2B
            </span>
          </div>
        </div>

        <!-- Usuario & Logout -->
        <div class="flex items-center gap-4">
          <div class="hidden sm:flex flex-col text-right">
            <span class="text-xs font-semibold text-slate-800 dark:text-slate-200">
              {{ userStore.email }}
            </span>
            <span class="text-[11px] text-slate-400 font-medium capitalize">
              Rol: {{ userStore.role || 'Client' }}
            </span>
          </div>
          <BaseButton variant="outline" @click="handleLogout">
            <template #icon>
              <LogOut class="w-4 h-4" />
            </template>
            Cerrar Sesión
          </BaseButton>
        </div>
      </div>
    </header>

    <!-- Contenido Principal -->
    <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Tarjeta de Bienvenida y Estado de Autenticación -->
      <div class="p-6 sm:p-8 rounded-3xl bg-gradient-to-r from-indigo-600 to-violet-700 text-white shadow-xl shadow-indigo-500/15 mb-8">
        <div class="flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
          <div>
            <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-white/15 text-xs font-semibold mb-3 backdrop-blur-xs">
              <ShieldCheck class="w-4 h-4 text-emerald-300" />
              Sesión Verificada con JWT
            </div>
            <h2 class="text-2xl sm:text-3xl font-extrabold tracking-tight">
              ¡Bienvenido a SweetTrack!
            </h2>
            <p class="text-indigo-100 text-sm mt-1 max-w-xl">
              Has iniciado sesión exitosamente como <strong>{{ userStore.email }}</strong> con rol <strong>{{ userStore.role }}</strong>. Ecosistema de compras y despacho conectado.
            </p>
          </div>

          <div class="p-4 rounded-2xl bg-white/10 border border-white/15 backdrop-blur-md text-xs font-mono max-w-sm w-full">
            <div class="flex items-center gap-2 mb-1.5 text-indigo-200 font-semibold">
              <Key class="w-3.5 h-3.5" />
              Token Bearer Activo
            </div>
            <p class="text-[11px] text-white/80 break-all line-clamp-2">
              {{ userStore.token }}
            </p>
          </div>
        </div>
      </div>

      <!-- Módulos de Acceso Rápido B2B -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <!-- Card 1: Catálogo (Activo) -->
        <div
          class="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs hover:shadow-lg hover:border-amber-500/50 dark:hover:border-amber-500/50 transition-all cursor-pointer group flex flex-col justify-between"
          @click="router.push('/catalog')"
        >
          <div>
            <div class="w-12 h-12 rounded-xl bg-amber-500/15 text-amber-600 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
              <Package class="w-6 h-6" />
            </div>
            <div class="flex items-center justify-between mb-1">
              <h3 class="font-bold text-base text-slate-900 dark:text-slate-100 group-hover:text-amber-600 dark:group-hover:text-amber-400 transition-colors">
                Catálogo de Golosinas
              </h3>
              <span class="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800">
                Disponible
              </span>
            </div>
            <p class="text-xs text-slate-500 dark:text-slate-400">
              Explora marcas líderes: Arcor, Terrabusi, Bagley y consulta precios mayoristas por bulto.
            </p>
          </div>
          <div class="mt-4 pt-3 border-t border-slate-100 dark:border-slate-800/80 flex items-center justify-between text-xs text-amber-600 font-bold">
            <span>Ver Catálogo</span>
            <span class="group-hover:translate-x-1 transition-transform">→</span>
          </div>
        </div>

        <!-- Card 2: Mis Pedidos (Activo) -->
        <div
          class="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs hover:shadow-lg hover:border-emerald-500/50 dark:hover:border-emerald-500/50 transition-all cursor-pointer group flex flex-col justify-between"
          @click="router.push('/my-orders')"
        >
          <div>
            <div class="w-12 h-12 rounded-xl bg-emerald-500/15 text-emerald-600 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
              <TrendingUp class="w-6 h-6" />
            </div>
            <div class="flex items-center justify-between mb-1">
              <h3 class="font-bold text-base text-slate-900 dark:text-slate-100 group-hover:text-emerald-600 dark:group-hover:text-emerald-400 transition-colors">
                Mis Pedidos y Envíos
              </h3>
              <span class="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800">
                En Vivo
              </span>
            </div>
            <p class="text-xs text-slate-500 dark:text-slate-400">
              Monitoreo en tiempo real del ciclo de vida de tus pedidos con stepper de entrega y remitos.
            </p>
          </div>
          <div class="mt-4 pt-3 border-t border-slate-100 dark:border-slate-800/80 flex items-center justify-between text-xs text-emerald-600 font-bold">
            <span>Ver Mis Pedidos</span>
            <span class="group-hover:translate-x-1 transition-transform">→</span>
          </div>
        </div>

        <!-- Card 3: Tablero de Despacho (Admin) / Carrito (Kiosco) -->
        <div
          v-if="userStore.isAdmin"
          class="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs hover:shadow-lg hover:border-indigo-500/50 dark:hover:border-indigo-500/50 transition-all cursor-pointer group flex flex-col justify-between"
          @click="router.push('/dispatch')"
        >
          <div>
            <div class="w-12 h-12 rounded-xl bg-indigo-500/15 text-indigo-600 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
              <Truck class="w-6 h-6" />
            </div>
            <div class="flex items-center justify-between mb-1">
              <h3 class="font-bold text-base text-slate-900 dark:text-slate-100 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">
                Tablero de Despacho
              </h3>
              <span class="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-indigo-50 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800">
                Depósito
              </span>
            </div>
            <p class="text-xs text-slate-500 dark:text-slate-400">
              Kanban logístico en tiempo real, asignación de rutas y emisión de remitos PDF con código de barras.
            </p>
          </div>
          <div class="mt-4 pt-3 border-t border-slate-100 dark:border-slate-800/80 flex items-center justify-between text-xs text-indigo-600 font-bold">
            <span>Abrir Despacho</span>
            <span class="group-hover:translate-x-1 transition-transform">→</span>
          </div>
        </div>

        <div
          v-else
          class="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs hover:shadow-lg hover:border-indigo-500/50 dark:hover:border-indigo-500/50 transition-all cursor-pointer group flex flex-col justify-between"
          @click="router.push('/catalog')"
        >
          <div>
            <div class="w-12 h-12 rounded-xl bg-indigo-500/15 text-indigo-600 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
              <ShoppingCart class="w-6 h-6" />
            </div>
            <div class="flex items-center justify-between mb-1">
              <h3 class="font-bold text-base text-slate-900 dark:text-slate-100 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">
                Carrito B2B
              </h3>
              <span class="text-[10px] font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800">
                Activo
              </span>
            </div>
            <p class="text-xs text-slate-500 dark:text-slate-400">
              Arma tu pedido mayorista con cálculo automático de bultos y validación de compra mínima ($50.000).
            </p>
          </div>
          <div class="mt-4 pt-3 border-t border-slate-100 dark:border-slate-800/80 flex items-center justify-between text-xs text-indigo-600 font-bold">
            <span>Ir al Carrito</span>
            <span class="group-hover:translate-x-1 transition-transform">→</span>
          </div>
        </div>

        <!-- Card 4: Perfil de Kiosco -->
        <div class="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm hover:shadow-md transition-shadow">
          <div class="w-12 h-12 rounded-xl bg-violet-500/15 text-violet-600 flex items-center justify-center mb-4">
            <User class="w-6 h-6" />
          </div>
          <h3 class="font-bold text-base mb-1">Perfil del Kiosco</h3>
          <p class="text-xs text-slate-500 dark:text-slate-400">
            Administra tus datos de facturación (CUIT), dirección de entrega y límites de crédito.
          </p>
        </div>
      </div>
    </main>
  </div>
</template>
