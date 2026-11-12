<script setup lang="ts">
import { computed } from 'vue'
import { useCartStore } from '@/stores/cartStore'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'
import { X, Trash2, ShoppingBag, ArrowRight } from '@lucide/vue'

const emit = defineEmits<{
  (e: 'checkout'): void
}>()

const cartStore = useCartStore()

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 2,
  }).format(amount)
}

const progressWidth = computed(() => `${cartStore.progressPercent}%`)

function handleProceedCheckout() {
  if (!cartStore.isMinimumMet) return
  emit('checkout')
}
</script>

<template>
  <div v-if="cartStore.isCartOpen" class="fixed inset-0 z-50 overflow-hidden">
    <!-- Backdrop -->
    <div
      class="fixed inset-0 bg-black/50 transition-opacity backdrop-blur-xs"
      @click="cartStore.closeCart"
    />

    <div class="fixed inset-y-0 right-0 max-w-full flex pl-10">
      <div class="w-screen max-w-md bg-white dark:bg-gray-900 shadow-2xl flex flex-col">
        <!-- Header -->
        <div class="p-6 border-b border-gray-200 dark:border-gray-800 flex items-center justify-between">
          <div class="flex items-center gap-2">
            <ShoppingBag class="w-6 h-6 text-brand-600 dark:text-brand-400" />
            <h2 class="text-xl font-bold text-gray-900 dark:text-gray-100">
              Carrito Mayorista
            </h2>
            <span
              v-if="cartStore.itemCount > 0"
              class="px-2 py-0.5 text-xs font-semibold rounded-full bg-brand-100 dark:bg-brand-900/40 text-brand-700 dark:text-brand-300"
            >
              {{ cartStore.itemCount }}
            </span>
          </div>
          <button
            class="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            @click="cartStore.closeCart"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Barra de Progreso del Mínimo Mayorista -->
        <div class="px-6 py-4 bg-gray-50 dark:bg-gray-800/50 border-b border-gray-200 dark:border-gray-800">
          <div class="flex items-center justify-between text-xs mb-1.5 font-medium">
            <span class="text-gray-600 dark:text-gray-400">Compra Mínima B2B:</span>
            <span
              :class="cartStore.isMinimumMet ? 'text-emerald-600 dark:text-emerald-400 font-bold' : 'text-amber-600 dark:text-amber-400'"
            >
              {{ cartStore.isMinimumMet ? '¡Monto Mínimo Alcanzado!' : `Faltan ${formatCurrency(cartStore.remainingForMinimum)}` }}
            </span>
          </div>
          <div class="w-full bg-gray-200 dark:bg-gray-700 h-2.5 rounded-full overflow-hidden">
            <div
              class="h-full transition-all duration-500 rounded-full"
              :class="cartStore.isMinimumMet ? 'bg-emerald-500' : 'bg-brand-500'"
              :style="{ width: progressWidth }"
            />
          </div>
          <p class="text-[11px] text-gray-500 dark:text-gray-400 mt-1">
            Monto mínimo requerido: {{ formatCurrency(cartStore.minimumOrderAmount) }}
          </p>
        </div>

        <!-- Lista de Productos -->
        <div class="flex-1 overflow-y-auto p-6 space-y-4">
          <div v-if="cartStore.items.length === 0" class="text-center py-16">
            <ShoppingBag class="w-16 h-16 mx-auto text-gray-300 dark:text-gray-700 mb-4" />
            <h3 class="text-base font-medium text-gray-900 dark:text-gray-200 mb-1">
              Tu carrito está vacío
            </h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-6">
              Agrega bultos o unidades desde el catálogo para iniciar tu pedido.
            </p>
            <BaseButton variant="outline" @click="cartStore.closeCart">
              Explorar Catálogo
            </BaseButton>
          </div>

          <div
            v-for="item in cartStore.items"
            :key="item.productId"
            class="p-4 rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-800/40 space-y-3"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <h4 class="font-semibold text-sm text-gray-900 dark:text-gray-100 line-clamp-1">
                  {{ item.productName }}
                </h4>
                <p class="text-xs text-gray-500 dark:text-gray-400 font-mono">
                  {{ item.barcode }}
                </p>
              </div>
              <button
                class="text-gray-400 hover:text-rose-500 transition-colors p-1"
                title="Eliminar producto"
                @click="cartStore.removeItem(item.productId)"
              >
                <Trash2 class="w-4 h-4" />
              </button>
            </div>

            <div class="flex items-center justify-between text-xs text-gray-600 dark:text-gray-300 pt-1">
              <div>
                <span class="font-medium text-gray-900 dark:text-gray-100">{{ item.boxes }}</span> bultos
                <span class="text-gray-400 mx-1">·</span>
                <span class="font-medium text-gray-900 dark:text-gray-100">{{ item.units }}</span> u. totales
              </div>
              <div class="text-right">
                <span class="font-bold text-sm text-gray-900 dark:text-gray-100">
                  {{ formatCurrency(item.subtotal) }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer y Totales -->
        <div v-if="cartStore.items.length > 0" class="p-6 border-t border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 space-y-4">
          <div class="space-y-1.5 text-sm">
            <div class="flex justify-between text-gray-600 dark:text-gray-400">
              <span>Subtotal Neto</span>
              <span>{{ formatCurrency(cartStore.subtotal) }}</span>
            </div>
            <div class="flex justify-between text-gray-600 dark:text-gray-400">
              <span>IVA (21%)</span>
              <span>{{ formatCurrency(cartStore.vatAmount) }}</span>
            </div>
            <div class="flex justify-between text-base font-bold text-gray-900 dark:text-gray-100 pt-2 border-t border-gray-200 dark:border-gray-700">
              <span>Total Estimado</span>
              <span class="text-brand-600 dark:text-brand-400 font-extrabold text-lg">
                {{ formatCurrency(cartStore.totalAmount) }}
              </span>
            </div>
          </div>

          <BaseAlert
            v-if="!cartStore.isMinimumMet"
            type="warning"
            :message="`Debes alcanzar ${formatCurrency(cartStore.minimumOrderAmount)} para emitir tu orden.`"
          />

          <BaseButton
            class="w-full justify-center text-base py-3"
            :disabled="!cartStore.isMinimumMet"
            @click="handleProceedCheckout"
          >
            <span>Continuar al Pedido</span>
            <ArrowRight class="w-4 h-4 ml-2" />
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>
