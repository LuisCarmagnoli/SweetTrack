<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  ArrowLeft,
  Truck,
  RotateCcw,
  FileText,
  CheckCircle2,
  AlertCircle,
  PackageCheck,
  Radio,
  Clock,
} from '@lucide/vue'
import { useOrderStore } from '@/stores/orderStore'
import { OrderStatus } from '@/types/ordering'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'

const router = useRouter()
const orderStore = useOrderStore()

const isProcessing = ref<string | null>(null)
const actionError = ref<string | null>(null)

onMounted(async () => {
  await orderStore.fetchDispatchBoard()
  await orderStore.initRealtime()
})

onUnmounted(async () => {
  await orderStore.teardownRealtime()
})

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 2,
  }).format(amount)
}

async function handleAdvanceStatus(orderId: string, nextStatus: OrderStatus, note: string) {
  isProcessing.value = orderId
  actionError.value = null
  try {
    await orderStore.transitionOrderStatus(orderId, nextStatus, note)
  } catch (err: any) {
    actionError.value = err.message || 'Error al cambiar estado del pedido.'
  } finally {
    isProcessing.value = null
  }
}

async function handleDownloadPdf(orderId: string, orderNumber: string) {
  isProcessing.value = orderId
  actionError.value = null
  try {
    await orderStore.downloadWaybillPdf(orderId, orderNumber)
  } catch (err: any) {
    actionError.value = err.message || 'Error al generar el remito PDF.'
  } finally {
    isProcessing.value = null
  }
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-800 dark:text-slate-200">
    <!-- Header Superior -->
    <header class="sticky top-0 z-30 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md border-b border-slate-200 dark:border-slate-800">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <div class="flex items-center gap-3">
          <button
            type="button"
            class="p-2 rounded-xl text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            title="Volver al Catálogo"
            @click="router.push('/catalog')"
          >
            <ArrowLeft class="w-5 h-5" />
          </button>
          <div>
            <h1 class="font-extrabold text-base tracking-tight leading-none text-slate-900 dark:text-slate-100">
              Tablero de Despacho y Logística
            </h1>
            <span class="text-[11px] text-slate-400">Control de Depósito y Remitos B2B</span>
          </div>
        </div>

        <div class="flex items-center gap-2">
          <!-- Indicador de Tiempo Real -->
          <div class="flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-400 border border-emerald-200 dark:border-emerald-800/60">
            <Radio class="w-3.5 h-3.5 animate-pulse text-emerald-500" />
            <span class="hidden sm:inline">Depósito en Vivo</span>
          </div>

          <BaseButton variant="outline" size="sm" @click="orderStore.fetchDispatchBoard()">
            <RotateCcw class="w-4 h-4 mr-1" />
            <span class="hidden sm:inline">Actualizar Tablero</span>
          </BaseButton>
        </div>
      </div>
    </header>

    <!-- Contenido Principal: Tablero Kanban -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      <BaseAlert v-if="actionError" type="error" :message="actionError" dismissible @dismiss="actionError = null" />

      <div v-if="orderStore.isLoading" class="text-center py-16">
        <Truck class="w-12 h-12 mx-auto text-slate-400 animate-bounce mb-3" />
        <p class="text-sm font-medium text-slate-500">Cargando etapas de despacho...</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-6 items-start">
        <!-- Columna 1: En Preparación -->
        <div class="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-4 shadow-xs">
          <div class="flex items-center justify-between pb-3 mb-4 border-b border-slate-100 dark:border-slate-800">
            <div class="flex items-center gap-2 font-bold text-sm text-blue-600 dark:text-blue-400">
              <Clock class="w-4 h-4" />
              <span>En Preparación</span>
            </div>
            <span class="px-2 py-0.5 rounded-full text-xs font-black bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300">
              {{ orderStore.dispatchBoard?.enPreparacion.length || 0 }}
            </span>
          </div>

          <div class="space-y-3">
            <div
              v-for="order in orderStore.dispatchBoard?.enPreparacion"
              :key="order.id"
              class="p-4 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/60 dark:bg-slate-800/40 space-y-3"
            >
              <div class="flex justify-between items-start">
                <span class="text-xs font-mono font-bold text-brand-600 dark:text-brand-400">
                  {{ order.orderNumber }}
                </span>
                <span class="text-xs font-extrabold text-slate-900 dark:text-slate-100">
                  {{ formatCurrency(order.totalAmount) }}
                </span>
              </div>

              <div>
                <h4 class="font-bold text-sm text-slate-900 dark:text-slate-100 line-clamp-1">
                  {{ order.kioskBusinessName }}
                </h4>
                <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                  {{ order.totalItems }} unidades solicitadas
                </p>
              </div>

              <div class="pt-2 border-t border-slate-200 dark:border-slate-700/60">
                <BaseButton
                  size="sm"
                  class="w-full justify-center"
                  :disabled="isProcessing === order.id"
                  @click="handleAdvanceStatus(order.id, OrderStatus.ListoParaDespacho, 'Mercadería empaquetada y lista para retiro')"
                >
                  <PackageCheck class="w-4 h-4 mr-1.5" />
                  Listo para Despacho
                </BaseButton>
              </div>
            </div>

            <div v-if="orderStore.dispatchBoard?.enPreparacion.length === 0" class="text-center py-10 text-xs text-slate-400">
              No hay pedidos en preparación.
            </div>
          </div>
        </div>

        <!-- Columna 2: Listo para Despacho -->
        <div class="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-4 shadow-xs">
          <div class="flex items-center justify-between pb-3 mb-4 border-b border-slate-100 dark:border-slate-800">
            <div class="flex items-center gap-2 font-bold text-sm text-indigo-600 dark:text-indigo-400">
              <PackageCheck class="w-4 h-4" />
              <span>Listos para Despacho</span>
            </div>
            <span class="px-2 py-0.5 rounded-full text-xs font-black bg-indigo-100 dark:bg-indigo-900/40 text-indigo-700 dark:text-indigo-300">
              {{ orderStore.dispatchBoard?.listoParaDespacho.length || 0 }}
            </span>
          </div>

          <div class="space-y-3">
            <div
              v-for="order in orderStore.dispatchBoard?.listoParaDespacho"
              :key="order.id"
              class="p-4 rounded-xl border border-indigo-200 dark:border-indigo-900/50 bg-indigo-50/20 dark:bg-indigo-950/20 space-y-3"
            >
              <div class="flex justify-between items-start">
                <span class="text-xs font-mono font-bold text-indigo-600 dark:text-indigo-400">
                  {{ order.orderNumber }}
                </span>
                <span class="text-xs font-extrabold text-slate-900 dark:text-slate-100">
                  {{ formatCurrency(order.totalAmount) }}
                </span>
              </div>

              <div>
                <h4 class="font-bold text-sm text-slate-900 dark:text-slate-100 line-clamp-1">
                  {{ order.kioskBusinessName }}
                </h4>
                <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                  {{ order.totalItems }} unidades
                </p>
              </div>

              <div class="pt-2 border-t border-indigo-100 dark:border-indigo-900/40 flex flex-col gap-2">
                <BaseButton
                  size="sm"
                  class="w-full justify-center"
                  :disabled="isProcessing === order.id"
                  @click="handleAdvanceStatus(order.id, OrderStatus.EnReparto, 'Cargado en vehículo de reparto')"
                >
                  <Truck class="w-4 h-4 mr-1.5" />
                  Despachar a Reparto
                </BaseButton>

                <BaseButton
                  variant="outline"
                  size="sm"
                  class="w-full justify-center"
                  :disabled="isProcessing === order.id"
                  @click="handleDownloadPdf(order.id, order.orderNumber)"
                >
                  <FileText class="w-4 h-4 mr-1.5 text-indigo-600" />
                  Descargar Remito PDF
                </BaseButton>
              </div>
            </div>

            <div v-if="orderStore.dispatchBoard?.listoParaDespacho.length === 0" class="text-center py-10 text-xs text-slate-400">
              No hay pedidos listos para despacho.
            </div>
          </div>
        </div>

        <!-- Columna 3: En Reparto -->
        <div class="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-4 shadow-xs">
          <div class="flex items-center justify-between pb-3 mb-4 border-b border-slate-100 dark:border-slate-800">
            <div class="flex items-center gap-2 font-bold text-sm text-amber-600 dark:text-amber-400">
              <Truck class="w-4 h-4" />
              <span>En Reparto</span>
            </div>
            <span class="px-2 py-0.5 rounded-full text-xs font-black bg-amber-100 dark:bg-amber-900/40 text-amber-700 dark:text-amber-300">
              {{ orderStore.dispatchBoard?.enReparto.length || 0 }}
            </span>
          </div>

          <div class="space-y-3">
            <div
              v-for="order in orderStore.dispatchBoard?.enReparto"
              :key="order.id"
              class="p-4 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50/60 dark:bg-slate-800/40 space-y-3"
            >
              <div class="flex justify-between items-start">
                <span class="text-xs font-mono font-bold text-amber-600 dark:text-amber-400">
                  {{ order.orderNumber }}
                </span>
                <span class="text-xs font-extrabold text-slate-900 dark:text-slate-100">
                  {{ formatCurrency(order.totalAmount) }}
                </span>
              </div>

              <div>
                <h4 class="font-bold text-sm text-slate-900 dark:text-slate-100 line-clamp-1">
                  {{ order.kioskBusinessName }}
                </h4>
                <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">
                  En camioneta con chofer
                </p>
              </div>

              <div class="pt-2 border-t border-slate-200 dark:border-slate-700/60 flex flex-col gap-2">
                <div class="grid grid-cols-2 gap-2">
                  <BaseButton
                    size="sm"
                    class="justify-center bg-emerald-600 hover:bg-emerald-700"
                    :disabled="isProcessing === order.id"
                    @click="handleAdvanceStatus(order.id, OrderStatus.Entregado, 'Entregado conforme y remito firmado')"
                  >
                    <CheckCircle2 class="w-4 h-4 mr-1" />
                    Entregado
                  </BaseButton>

                  <BaseButton
                    size="sm"
                    class="justify-center bg-rose-600 hover:bg-rose-700 text-white"
                    :disabled="isProcessing === order.id"
                    @click="handleAdvanceStatus(order.id, OrderStatus.NoEntregado, 'Kiosco cerrado en horario de visita')"
                  >
                    <AlertCircle class="w-4 h-4 mr-1" />
                    No Entregado
                  </BaseButton>
                </div>

                <BaseButton
                  variant="outline"
                  size="sm"
                  class="w-full justify-center"
                  :disabled="isProcessing === order.id"
                  @click="handleDownloadPdf(order.id, order.orderNumber)"
                >
                  <FileText class="w-4 h-4 mr-1.5 text-indigo-600" />
                  Descargar Remito PDF
                </BaseButton>
              </div>
            </div>

            <div v-if="orderStore.dispatchBoard?.enReparto.length === 0" class="text-center py-10 text-xs text-slate-400">
              No hay pedidos en reparto.
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>
