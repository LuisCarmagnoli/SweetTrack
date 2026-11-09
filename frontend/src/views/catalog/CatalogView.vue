<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  ArrowLeft,
  RefreshCw,
  UploadCloud,
  Package,
  AlertTriangle,
  ChevronLeft,
  ChevronRight,
  Store,
} from '@lucide/vue'
import { useCatalogStore } from '@/stores/catalogStore'
import { useUserStore } from '@/stores/userStore'
import type { ProductResponseDto } from '@/types/catalog'
import ProductCard from '@/components/catalog/ProductCard.vue'
import CatalogFilters from '@/components/catalog/CatalogFilters.vue'
import StockAdjustmentModal from '@/components/catalog/StockAdjustmentModal.vue'
import BulkImportModal from '@/components/catalog/BulkImportModal.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'

const router = useRouter()
const catalogStore = useCatalogStore()
const userStore = useUserStore()

// Modales
const isStockModalOpen = ref(false)
const selectedProductForAdjustment = ref<ProductResponseDto | null>(null)
const isBulkImportModalOpen = ref(false)
const successNotification = ref<string | null>(null)

onMounted(() => {
  catalogStore.fetchMetadata()
  catalogStore.fetchProducts()
})

function handleOpenAdjustment(product: ProductResponseDto) {
  selectedProductForAdjustment.value = product
  isStockModalOpen.value = true
}

function handleAdjustmentSuccess() {
  successNotification.value = 'Stock ajustado y registrado en auditoría correctamente.'
  setTimeout(() => {
    successNotification.value = null
  }, 4000)
}

function handleBulkImportCompleted() {
  successNotification.value = 'Planilla Excel procesada. Catálogo actualizado con éxito.'
  setTimeout(() => {
    successNotification.value = null
  }, 4000)
}

function changePage(newPage: number) {
  if (newPage < 1 || newPage > catalogStore.totalPages) return
  catalogStore.fetchProducts(newPage)
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 flex flex-col">
    <!-- Navbar / Barra Superior -->
    <header class="border-b border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900/90 backdrop-blur-md sticky top-0 z-20">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <div class="flex items-center gap-3">
          <button
            type="button"
            class="w-9 h-9 rounded-xl flex items-center justify-center text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            title="Volver al Dashboard"
            @click="router.push('/dashboard')"
          >
            <ArrowLeft class="w-5 h-5" />
          </button>

          <div class="flex items-center gap-2">
            <div class="w-8 h-8 rounded-lg bg-amber-500/15 text-amber-600 flex items-center justify-center font-bold">
              <Package class="w-4 h-4" />
            </div>
            <div>
              <h1 class="font-extrabold text-base tracking-tight leading-none">
                Catálogo Mayorista
              </h1>
              <span class="text-[11px] text-slate-400">SweetTrack B2B</span>
            </div>
          </div>
        </div>

        <div class="flex items-center gap-2">
          <!-- Botón Admin: Importar Excel -->
          <BaseButton
            v-if="userStore.isAdmin"
            variant="outline"
            size="sm"
            @click="isBulkImportModalOpen = true"
          >
            <template #icon>
              <UploadCloud class="w-4 h-4 text-indigo-600" />
            </template>
            <span class="hidden sm:inline">Importar Excel</span>
          </BaseButton>

          <!-- Botón Refrescar -->
          <button
            type="button"
            class="w-9 h-9 rounded-xl flex items-center justify-center text-slate-500 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-100 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            :class="{ 'animate-spin': catalogStore.isLoading }"
            title="Recargar catálogo"
            @click="catalogStore.fetchProducts()"
          >
            <RefreshCw class="w-4 h-4" />
          </button>
        </div>
      </div>
    </header>

    <!-- Contenido Principal -->
    <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 sm:py-8">
      <!-- Notificación de Éxito Flotante -->
      <div v-if="successNotification" class="mb-6">
        <BaseAlert
          variant="success"
          :message="successNotification"
          dismissible
          @dismiss="successNotification = null"
        />
      </div>

      <!-- Alerta de Error de Red / Carga -->
      <div v-if="catalogStore.errorMessage" class="mb-6">
        <BaseAlert
          variant="error"
          :message="catalogStore.errorMessage"
          dismissible
          @dismiss="catalogStore.errorMessage = null"
        />
      </div>

      <!-- Barra de Estadísticas Rápidas -->
      <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
        <div>
          <h2 class="text-2xl sm:text-3xl font-extrabold tracking-tight">
            Golosinas y Snacks Líderes
          </h2>
          <p class="text-xs sm:text-sm text-slate-500 dark:text-slate-400 mt-0.5">
            Precios mayoristas por bulto, marcas argentinas y existencias de depósito en tiempo real.
          </p>
        </div>

        <!-- Badges de métricas -->
        <div class="flex items-center gap-2">
          <div class="px-3 py-1 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs font-semibold">
            <span class="text-slate-400">Total:</span>
            <strong class="ml-1 text-slate-900 dark:text-slate-100">{{ catalogStore.totalCount }} golosinas</strong>
          </div>
          <div
            v-if="catalogStore.lowStockCount > 0"
            class="px-3 py-1 rounded-xl bg-amber-50 dark:bg-amber-950/60 border border-amber-200 dark:border-amber-800/80 text-xs font-bold text-amber-700 dark:text-amber-300 inline-flex items-center gap-1.5"
          >
            <AlertTriangle class="w-3.5 h-3.5" />
            <span>{{ catalogStore.lowStockCount }} stock crítico</span>
          </div>
        </div>
      </div>

      <!-- Componente de Filtros y Búsqueda -->
      <CatalogFilters
        :categories="catalogStore.categories"
        :brands="catalogStore.brands"
        :search-term="catalogStore.filters.searchTerm"
        :selected-category="catalogStore.filters.categoryId"
        :selected-brand="catalogStore.filters.brandId"
        :selected-order-by="catalogStore.filters.orderBy"
        :only-in-stock="catalogStore.filters.onlyInStock"
        @update:search-term="catalogStore.setSearchTerm"
        @update:selected-category="catalogStore.setCategory"
        @update:selected-brand="catalogStore.setBrand"
        @update:selected-order-by="catalogStore.setOrderBy"
        @update:only-in-stock="catalogStore.setStockFilter"
        @reset="catalogStore.resetFilters"
      />

      <!-- Grilla de Productos -->
      <div v-if="catalogStore.isLoading" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
        <!-- Skeleton Loader -->
        <div
          v-for="n in 8"
          :key="n"
          class="rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 space-y-4 animate-pulse"
        >
          <div class="h-36 bg-slate-200 dark:bg-slate-800 rounded-xl" />
          <div class="h-4 bg-slate-200 dark:bg-slate-800 rounded w-3/4" />
          <div class="h-4 bg-slate-200 dark:bg-slate-800 rounded w-1/2" />
          <div class="h-8 bg-slate-200 dark:bg-slate-800 rounded-xl" />
        </div>
      </div>

      <!-- Estado Vacío (Sin resultados) -->
      <div
        v-else-if="!catalogStore.hasProducts"
        class="py-16 text-center rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-8 space-y-4 max-w-md mx-auto"
      >
        <div class="w-16 h-16 rounded-3xl bg-amber-50 dark:bg-amber-950 text-amber-600 dark:text-amber-400 flex items-center justify-center mx-auto">
          <Store class="w-8 h-8 opacity-80" />
        </div>
        <h3 class="font-extrabold text-lg text-slate-800 dark:text-slate-200">
          No se encontraron golosinas
        </h3>
        <p class="text-xs text-slate-500 dark:text-slate-400">
          No hay productos que coincidan con los filtros o término de búsqueda ingresados.
        </p>
        <BaseButton variant="outline" size="sm" @click="catalogStore.resetFilters">
          Restablecer Filtros
        </BaseButton>
      </div>

      <!-- Lista de Productos -->
      <div
        v-else
        class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
      >
        <ProductCard
          v-for="product in catalogStore.products"
          :key="product.id"
          :product="product"
          :is-admin="userStore.isAdmin"
          @adjust-stock="handleOpenAdjustment"
        />
      </div>

      <!-- Barra de Paginación -->
      <div
        v-if="catalogStore.totalPages > 1"
        class="mt-10 pt-6 border-t border-slate-200 dark:border-slate-800 flex flex-col sm:flex-row items-center justify-between gap-4 text-xs text-slate-500 dark:text-slate-400"
      >
        <div>
          Página <strong class="text-slate-800 dark:text-slate-200">{{ catalogStore.page }}</strong> de
          <strong class="text-slate-800 dark:text-slate-200">{{ catalogStore.totalPages }}</strong>
          (Total: {{ catalogStore.totalCount }} productos)
        </div>

        <div class="flex items-center gap-2">
          <BaseButton
            variant="outline"
            size="sm"
            :disabled="!catalogStore.hasPreviousPage"
            @click="changePage(catalogStore.page - 1)"
          >
            <template #icon>
              <ChevronLeft class="w-4 h-4" />
            </template>
            Anterior
          </BaseButton>

          <BaseButton
            variant="outline"
            size="sm"
            :disabled="!catalogStore.hasNextPage"
            @click="changePage(catalogStore.page + 1)"
          >
            Siguiente
            <template #icon>
              <ChevronRight class="w-4 h-4" />
            </template>
          </BaseButton>
        </div>
      </div>
    </main>

    <!-- Modal de Ajuste de Stock (Admin) -->
    <StockAdjustmentModal
      v-model="isStockModalOpen"
      :product="selectedProductForAdjustment"
      @success="handleAdjustmentSuccess"
    />

    <!-- Modal de Importación Masiva Excel (Admin) -->
    <BulkImportModal
      v-model="isBulkImportModalOpen"
      @completed="handleBulkImportCompleted"
    />
  </div>
</template>
