<script setup lang="ts">
import { Store, Sparkles } from '@lucide/vue'

defineProps<{
  activeTab: 'login' | 'register'
}>()

const emit = defineEmits<{
  (e: 'update:activeTab', tab: 'login' | 'register'): void
}>()
</script>

<template>
  <div class="w-full max-w-md mx-auto">
    <!-- Header de Branding -->
    <div class="text-center mb-8">
      <div class="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-tr from-indigo-600 via-violet-600 to-amber-500 shadow-lg shadow-indigo-500/25 mb-4 text-white">
        <Store class="w-8 h-8" />
      </div>
      <div class="flex items-center justify-center gap-2 mb-1">
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-slate-900 dark:text-white">
          SweetTrack
        </h1>
        <span class="inline-flex items-center gap-1 text-[11px] font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-amber-500/15 text-amber-700 dark:text-amber-400 border border-amber-500/30">
          <Sparkles class="w-3 h-3" /> B2B
        </span>
      </div>
      <p class="text-sm text-slate-500 dark:text-slate-400">
        Plataforma Mayorista para Kioscos y Distribuidoras
      </p>
    </div>

    <!-- Contenedor Principal con Tabs -->
    <div class="bg-white dark:bg-slate-900/90 rounded-3xl p-6 sm:p-8 shadow-xl shadow-slate-200/50 dark:shadow-black/40 border border-slate-100 dark:border-slate-800/80 backdrop-blur-md">
      <!-- Selector de Pestañas (Tabs) -->
      <div class="grid grid-cols-2 p-1 mb-6 rounded-xl bg-slate-100 dark:bg-slate-800/80 text-xs font-semibold">
        <button
          type="button"
          :class="[
            'py-2.5 rounded-lg transition-all duration-200 cursor-pointer',
            activeTab === 'login'
              ? 'bg-white dark:bg-slate-900 text-indigo-600 dark:text-indigo-400 shadow-xs font-bold'
              : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white'
          ]"
          @click="emit('update:activeTab', 'login')"
        >
          Iniciar Sesión
        </button>
        <button
          type="button"
          :class="[
            'py-2.5 rounded-lg transition-all duration-200 cursor-pointer',
            activeTab === 'register'
              ? 'bg-white dark:bg-slate-900 text-indigo-600 dark:text-indigo-400 shadow-xs font-bold'
              : 'text-slate-600 dark:text-slate-400 hover:text-slate-900 dark:hover:text-white'
          ]"
          @click="emit('update:activeTab', 'register')"
        >
          Registrar Kiosco
        </button>
      </div>

      <!-- Contenido Inyectado -->
      <slot />
    </div>

    <!-- Footer Informativo -->
    <div class="mt-6 text-center text-xs text-slate-500 dark:text-slate-500">
      <p>© 2026 SweetTrack B2B · Distribución mayorista de golosinas y snacks</p>
    </div>
  </div>
</template>
