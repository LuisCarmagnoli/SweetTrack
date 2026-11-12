<script setup lang="ts">
import { computed } from 'vue'
import {
  Package,
  Layers,
  Barcode,
  SlidersHorizontal,
  ShoppingCart,
} from '@lucide/vue'
import { useCartStore } from '@/stores/cartStore'
import type { ProductResponseDto } from '@/types/catalog'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseButton from '@/components/base/BaseButton.vue'

interface Props {
  product: ProductResponseDto
  isAdmin?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isAdmin: false,
})

const emit = defineEmits<{
  (e: 'adjust-stock', product: ProductResponseDto): void
}>()

const cartStore = useCartStore()

function handleAddBox() {
  cartStore.addOrUpdateProduct(props.product, 1, 0)
  cartStore.openCart()
}

// Formateador monetario argentino
const currencyFormatter = new Intl.NumberFormat('es-AR', {
  style: 'currency',
  currency: 'ARS',
  minimumFractionDigits: 2,
})

const formattedWholesalePrice = computed(() => {
  return currencyFormatter.format(props.product.wholesalePrice)
})

const formattedBoxPrice = computed(() => {
  return currencyFormatter.format(props.product.boxPrice)
})

const stockBadgeVariant = computed(() => {
  if (props.product.stockUnits === 0) return 'danger'
  if (props.product.isLowStock) return 'warning'
  return 'success'
})

const stockBadgeText = computed(() => {
  if (props.product.stockUnits === 0) return 'Sin Stock'
  if (props.product.isLowStock) return `Bajo Stock (${props.product.stockUnits})`
  return `${props.product.stockUnits} en stock`
})

// Gradiente temático para placeholder visual según categoría
const categoryGradient = computed(() => {
  const cat = props.product.categoryName.toLowerCase()
  if (cat.includes('chocolat')) return 'from-amber-800 to-amber-950 text-amber-200'
  if (cat.includes('alfajor')) return 'from-amber-600 to-orange-700 text-amber-100'
  if (cat.includes('caramel') || cat.includes('gomit')) return 'from-pink-500 to-rose-600 text-pink-100'
  if (cat.includes('galletit')) return 'from-yellow-600 to-amber-700 text-yellow-100'
  return 'from-indigo-600 to-slate-800 text-indigo-100'
})
</script>

<template>
  <div
    class="group rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs hover:shadow-md transition-all duration-200 flex flex-col overflow-hidden"
  >
    <!-- Imagen o Placeholder Visual de la Golosina -->
    <div
      class="h-40 w-full relative flex items-center justify-center bg-gradient-to-br overflow-hidden"
      :class="categoryGradient"
    >
      <img
        v-if="product.imageUrl"
        :src="product.imageUrl"
        :alt="product.name"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
      />
      <div v-else class="flex flex-col items-center gap-1 p-4 text-center">
        <Package class="w-10 h-10 opacity-80" />
        <span class="text-xs font-semibold uppercase tracking-wider opacity-90 line-clamp-1">
          {{ product.brandName }}
        </span>
      </div>

      <!-- Badge de Estado de Stock en esquina superior -->
      <div class="absolute top-3 right-3">
        <BaseBadge :variant="stockBadgeVariant" dot size="sm">
          {{ stockBadgeText }}
        </BaseBadge>
      </div>

      <!-- Tag de Categoría en esquina inferior izquierda -->
      <div class="absolute bottom-3 left-3">
        <span class="px-2 py-0.5 rounded-md text-[10px] font-bold bg-slate-900/70 text-white backdrop-blur-xs">
          {{ product.categoryName }}
        </span>
      </div>
    </div>

    <!-- Cuerpo de Información -->
    <div class="p-5 flex-1 flex flex-col justify-between">
      <div>
        <!-- Marca y Código de Barras -->
        <div class="flex items-center justify-between text-[11px] text-slate-400 font-mono mb-1.5">
          <span class="font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400">
            {{ product.brandName }}
          </span>
          <span class="flex items-center gap-1">
            <Barcode class="w-3.5 h-3.5" />
            {{ product.barcode }}
          </span>
        </div>

        <!-- Título del Producto -->
        <h4
          class="font-extrabold text-base text-slate-900 dark:text-slate-100 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors line-clamp-2 leading-snug mb-3"
          :title="product.name"
        >
          {{ product.name }}
        </h4>
      </div>

      <!-- Precios y Presentación B2B -->
      <div class="pt-3 border-t border-slate-100 dark:border-slate-800/80">
        <div class="flex items-baseline justify-between mb-1">
          <span class="text-xs text-slate-400 font-medium">Mayorista Unitario:</span>
          <span class="text-base font-extrabold text-slate-900 dark:text-slate-100">
            {{ formattedWholesalePrice }}
          </span>
        </div>

        <div class="flex items-center justify-between p-2 rounded-xl bg-slate-50 dark:bg-slate-800/50 text-xs">
          <div class="flex items-center gap-1.5 text-slate-500 dark:text-slate-400">
            <Layers class="w-3.5 h-3.5 text-amber-500" />
            <span>Caja x {{ product.unitsPerBox }} un.</span>
          </div>
          <span class="font-bold text-slate-800 dark:text-slate-200">
            {{ formattedBoxPrice }}
          </span>
        </div>

        <!-- Acciones: Botón de Compra Mayorista (Kiosco) o Ajuste de Stock (Admin) -->
        <div class="mt-3 flex items-center gap-2">
          <BaseButton
            v-if="isAdmin"
            variant="outline"
            size="sm"
            class="w-full justify-center"
            @click="emit('adjust-stock', product)"
          >
            <template #icon>
              <SlidersHorizontal class="w-3.5 h-3.5" />
            </template>
            Ajustar Stock
          </BaseButton>

          <BaseButton
            v-else
            size="sm"
            class="w-full justify-center"
            :disabled="product.stockUnits === 0"
            @click="handleAddBox"
          >
            <ShoppingCart class="w-3.5 h-3.5 mr-1" />
            <span>{{ product.stockUnits === 0 ? 'Agotado' : '+1 Bulto al Carrito' }}</span>
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>
