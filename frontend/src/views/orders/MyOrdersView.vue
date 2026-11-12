<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  ArrowLeft,
  Package,
  Calendar,
  CheckCircle2,
  XCircle,
  ChevronRight,
  Radio,
  RotateCcw,
} from '@lucide/vue'
import { useOrderStore } from '@/stores/orderStore'
import { OrderStatus, OrderStatusLabels, type OrderResponseDto } from '@/types/ordering'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'

const router = useRouter()
const orderStore = useOrderStore()

const selectedOrder = ref<any | null>(null)
const isDetailModalOpen = ref(false)
const cancelNotes = ref('')
const isCancelling = ref(false)
const actionError = ref<string | null>(null)

onMounted(async () => {
  await orderStore.fetchMyOrders()
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

const formatDate = (isoString: string) => {
  return new Date(isoString).toLocaleDateString('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const getStatusVariant = (status: OrderStatus): 'success' | 'warning' | 'danger' | 'info' | 'neutral' => {
  switch (status) {
    case OrderStatus.PendienteConfirmacion:
      return 'warning'
    case OrderStatus.EnPreparacion:
      return 'info'
    case OrderStatus.ListoParaDespacho:
      return 'info'
    case OrderStatus.EnReparto:
      return 'warning'
    case OrderStatus.Entregado:
      return 'success'
    case OrderStatus.Cancelado:
    case OrderStatus.NoEntregado:
      return 'danger'
    default:
      return 'neutral'
  }
}

// Pasos canónicos del stepper
const steps = [
  { status: OrderStatus.PendienteConfirmacion, label: 'Confirmado' },
  { status: OrderStatus.EnPreparacion, label: 'En Preparación' },
  { status: OrderStatus.ListoParaDespacho, label: 'Listo Despacho' },
  { status: OrderStatus.EnReparto, label: 'En Reparto' },
  { status: OrderStatus.Entregado, label: 'Entregado' },
]

function getStepState(currentStatus: OrderStatus, stepStatus: OrderStatus) {
  if (currentStatus === OrderStatus.Cancelado) return 'cancelled'
  if (currentStatus >= stepStatus) return 'completed'
  return 'pending'
}

async function handleViewDetail(order: OrderResponseDto) {
  await orderStore.fetchOrderById(order.id)
  selectedOrder.value = orderStore.currentOrder
  isDetailModalOpen.value = true
}

async function handleCancelOrder() {
  if (!selectedOrder.value) return
  isCancelling.value = true
  actionError.value = null
  try {
    await orderStore.transitionOrderStatus(
      selectedOrder.value.id,
      OrderStatus.Cancelado,
      cancelNotes.value || 'Cancelado por el cliente'
    )
    isDetailModalOpen.value = false
    cancelNotes.value = ''
  } catch (err: any) {
    actionError.value = err.message || 'No se pudo cancelar el pedido.'
  } finally {
    isCancelling.value = false
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
              Mis Pedidos Mayoristas
            </h1>
            <span class="text-[11px] text-slate-400">Historial y Seguimiento en Vivo</span>
          </div>
        </div>

        <div class="flex items-center gap-2">
          <!-- Indicador de Tiempo Real -->
          <div class="flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-400 border border-emerald-200 dark:border-emerald-800/60">
            <Radio class="w-3.5 h-3.5 animate-pulse text-emerald-500" />
            <span class="hidden sm:inline">En Vivo</span>
          </div>

          <BaseButton variant="outline" size="sm" @click="orderStore.fetchMyOrders()">
            <RotateCcw class="w-4 h-4 mr-1" />
            <span class="hidden sm:inline">Actualizar</span>
          </BaseButton>
        </div>
      </div>
    </header>

    <!-- Contenido Principal -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      <div v-if="orderStore.isLoading" class="text-center py-16">
        <Package class="w-12 h-12 mx-auto text-slate-400 animate-bounce mb-3" />
        <p class="text-sm font-medium text-slate-500">Cargando tus pedidos...</p>
      </div>

      <div v-else-if="orderStore.myOrders.length === 0" class="text-center py-20 bg-white dark:bg-slate-900 rounded-2xl border border-slate-200 dark:border-slate-800 p-8">
        <Package class="w-16 h-16 mx-auto text-slate-300 dark:text-slate-700 mb-4" />
        <h3 class="text-lg font-bold text-slate-900 dark:text-slate-100 mb-1">
          Aún no tienes pedidos registrados
        </h3>
        <p class="text-sm text-slate-500 dark:text-slate-400 mb-6 max-w-md mx-auto">
          Explora nuestro catálogo mayorista de golosinas líderes y emite tu primer pedido de reabastecimiento.
        </p>
        <BaseButton @click="router.push('/catalog')">
          Ir al Catálogo
        </BaseButton>
      </div>

      <!-- Listado de Tarjetas de Pedido -->
      <div v-else class="space-y-4">
        <div
          v-for="order in orderStore.myOrders"
          :key="order.id"
          class="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-6 shadow-xs hover:shadow-md transition-shadow"
        >
          <!-- Fila Superior: Info y Estado -->
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-4 border-b border-slate-100 dark:border-slate-800/80">
            <div>
              <div class="flex items-center gap-2">
                <span class="text-xs font-mono font-bold text-brand-600 dark:text-brand-400">
                  {{ order.orderNumber }}
                </span>
                <span class="text-xs text-slate-400">·</span>
                <span class="text-xs text-slate-500 dark:text-slate-400 flex items-center gap-1">
                  <Calendar class="w-3.5 h-3.5" />
                  {{ formatDate(order.createdAtUtc) }}
                </span>
              </div>
              <h3 class="font-extrabold text-base text-slate-900 dark:text-slate-100 mt-1">
                {{ order.kioskBusinessName }}
              </h3>
            </div>

            <div class="flex items-center gap-3">
              <BaseBadge :variant="getStatusVariant(order.status)" size="sm">
                {{ OrderStatusLabels[order.status] }}
              </BaseBadge>
              <span class="text-lg font-extrabold text-slate-900 dark:text-slate-100">
                {{ formatCurrency(order.totalAmount) }}
              </span>
            </div>
          </div>

          <!-- Stepper Visual Interactivo -->
          <div class="py-5 overflow-x-auto">
            <div class="min-w-[500px] flex items-center justify-between relative">
              <div
                v-for="(step, idx) in steps"
                :key="step.status"
                class="flex flex-col items-center relative z-10 flex-1"
              >
                <!-- Línea conectora -->
                <div
                  v-if="idx > 0"
                  class="absolute top-4 -left-1/2 w-full h-0.5 -z-10"
                  :class="getStepState(order.status, step.status) === 'completed' ? 'bg-emerald-500' : 'bg-slate-200 dark:bg-slate-800'"
                />

                <!-- Icono de estado -->
                <div
                  class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold transition-colors"
                  :class="{
                    'bg-emerald-500 text-white ring-4 ring-emerald-100 dark:ring-emerald-950': getStepState(order.status, step.status) === 'completed',
                    'bg-rose-500 text-white ring-4 ring-rose-100 dark:ring-rose-950': order.status === OrderStatus.Cancelado,
                    'bg-slate-200 dark:bg-slate-800 text-slate-500': getStepState(order.status, step.status) === 'pending'
                  }"
                >
                  <CheckCircle2 v-if="getStepState(order.status, step.status) === 'completed'" class="w-4 h-4" />
                  <XCircle v-else-if="order.status === OrderStatus.Cancelado" class="w-4 h-4" />
                  <span v-else>{{ idx + 1 }}</span>
                </div>

                <span
                  class="text-[11px] font-semibold mt-2 text-center"
                  :class="getStepState(order.status, step.status) === 'completed' ? 'text-slate-900 dark:text-slate-100' : 'text-slate-400'"
                >
                  {{ step.label }}
                </span>
              </div>
            </div>
          </div>

          <!-- Fila Inferior: Botón de Detalle -->
          <div class="flex items-center justify-between pt-4 border-t border-slate-100 dark:border-slate-800/80 text-xs text-slate-500">
            <span>{{ order.totalItems }} unidades totales</span>
            <BaseButton variant="outline" size="sm" @click="handleViewDetail(order)">
              <span>Ver Detalle y Desglose</span>
              <ChevronRight class="w-4 h-4 ml-1" />
            </BaseButton>
          </div>
        </div>
      </div>
    </main>

    <!-- Modal Detalle del Pedido -->
    <BaseModal
      v-if="selectedOrder"
      :model-value="isDetailModalOpen"
      :title="`Detalle del Pedido ${selectedOrder.orderNumber}`"
      @update:model-value="isDetailModalOpen = $event"
      @close="isDetailModalOpen = false"
    >
      <div class="space-y-4 text-sm">
        <BaseAlert v-if="actionError" type="error" :message="actionError" />

        <div class="grid grid-cols-2 gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
          <div>
            <span class="text-xs text-slate-400">Razón Social:</span>
            <p class="font-bold text-slate-900 dark:text-slate-100">{{ selectedOrder.kioskBusinessName }}</p>
          </div>
          <div>
            <span class="text-xs text-slate-400">Dirección de Entrega:</span>
            <p class="font-bold text-slate-900 dark:text-slate-100">{{ selectedOrder.deliveryAddress }}</p>
          </div>
        </div>

        <!-- Tabla de Productos del Pedido -->
        <div class="border border-slate-200 dark:border-slate-800 rounded-xl overflow-hidden">
          <table class="w-full text-left text-xs">
            <thead class="bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-300 font-bold">
              <tr>
                <th class="p-2.5">Producto</th>
                <th class="p-2.5 text-center">Bultos</th>
                <th class="p-2.5 text-center">Unidades</th>
                <th class="p-2.5 text-right">Subtotal</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 dark:divide-slate-800">
              <tr v-for="item in selectedOrder.items" :key="item.id">
                <td class="p-2.5 font-medium text-slate-900 dark:text-slate-100">
                  {{ item.productName }}
                  <span class="block text-[10px] text-slate-400 font-mono">{{ item.barcode }}</span>
                </td>
                <td class="p-2.5 text-center">{{ item.boxes }}</td>
                <td class="p-2.5 text-center">{{ item.units }}</td>
                <td class="p-2.5 text-right font-bold">{{ formatCurrency(item.lineTotal) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Totales -->
        <div class="p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50 space-y-1 text-right">
          <div class="text-xs text-slate-500">Subtotal: {{ formatCurrency(selectedOrder.subtotal) }}</div>
          <div class="text-xs text-slate-500">IVA (21%): {{ formatCurrency(selectedOrder.vatAmount) }}</div>
          <div class="text-base font-extrabold text-brand-600 dark:text-brand-400">
            Total: {{ formatCurrency(selectedOrder.totalAmount) }}
          </div>
        </div>

        <!-- Opción de Cancelar si está pendiente -->
        <div
          v-if="selectedOrder.status === OrderStatus.PendienteConfirmacion"
          class="p-4 rounded-xl border border-rose-200 dark:border-rose-900/60 bg-rose-50/50 dark:bg-rose-950/20 space-y-2"
        >
          <h4 class="font-bold text-xs text-rose-800 dark:text-rose-300">
            ¿Deseas cancelar este pedido?
          </h4>
          <input
            v-model="cancelNotes"
            type="text"
            placeholder="Motivo de la cancelación (opcional)"
            class="w-full text-xs px-3 py-1.5 rounded-lg border border-rose-300 dark:border-rose-800 bg-white dark:bg-slate-900 text-slate-800 dark:text-slate-200"
          />
          <button
            type="button"
            :disabled="isCancelling"
            class="inline-flex items-center justify-center gap-1.5 px-3 py-2 rounded-xl text-xs font-semibold bg-rose-600 hover:bg-rose-700 text-white disabled:opacity-50 transition-colors shadow-xs cursor-pointer"
            @click="handleCancelOrder"
          >
            <span v-if="isCancelling">Cancelando...</span>
            <span v-else>Confirmar Cancelación</span>
          </button>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end">
          <BaseButton variant="outline" @click="isDetailModalOpen = false">
            Cerrar
          </BaseButton>
        </div>
      </template>
    </BaseModal>
  </div>
</template>
