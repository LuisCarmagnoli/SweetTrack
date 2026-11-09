<script setup lang="ts">
import { ref, watch } from 'vue'
import {
  Search,
  X,
  RotateCcw,
} from '@lucide/vue'
import type { CategoryDto, BrandDto } from '@/types/catalog'
import BaseButton from '@/components/base/BaseButton.vue'

interface Props {
  categories: CategoryDto[]
  brands: BrandDto[]
  searchTerm?: string
  selectedCategory?: number | null
  selectedBrand?: number | null
  selectedOrderBy?: 'name_asc' | 'name_desc' | 'price_asc' | 'price_desc'
  onlyInStock?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  searchTerm: '',
  selectedCategory: null,
  selectedBrand: null,
  selectedOrderBy: undefined,
  onlyInStock: false,
})

const emit = defineEmits<{
  (e: 'update:searchTerm', val: string): void
  (e: 'update:selectedCategory', val: number | null): void
  (e: 'update:selectedBrand', val: number | null): void
  (e: 'update:selectedOrderBy', val: 'name_asc' | 'name_desc' | 'price_asc' | 'price_desc' | undefined): void
  (e: 'update:onlyInStock', val: boolean | undefined): void
  (e: 'reset'): void
}>()

const localSearch = ref(props.searchTerm)
let debounceTimer: ReturnType<typeof setTimeout> | null = null

watch(
  () => props.searchTerm,
  (newVal) => {
    localSearch.value = newVal
  }
)

function handleSearchInput(event: Event) {
  const target = event.target as HTMLInputElement
  localSearch.value = target.value

  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    emit('update:searchTerm', localSearch.value)
  }, 350)
}

function clearSearch() {
  localSearch.value = ''
  emit('update:searchTerm', '')
}

function handleCategoryChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const val = target.value ? Number(target.value) : null
  emit('update:selectedCategory', val)
}

function handleBrandChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const val = target.value ? Number(target.value) : null
  emit('update:selectedBrand', val)
}

function handleOrderChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const val = (target.value || undefined) as 'name_asc' | 'name_desc' | 'price_asc' | 'price_desc' | undefined
  emit('update:selectedOrderBy', val)
}

function toggleStockFilter() {
  emit('update:onlyInStock', !props.onlyInStock ? true : undefined)
}
</script>

<template>
  <div class="p-4 sm:p-5 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-xs mb-6 space-y-4">
    <div class="grid grid-cols-1 md:grid-cols-12 gap-3 sm:gap-4 items-center">
      <!-- Buscador de Texto / Código de Barras -->
      <div class="md:col-span-5 relative">
        <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
          <Search class="w-4 h-4" />
        </div>
        <input
          :value="localSearch"
          type="text"
          placeholder="Buscar por nombre o código de barras..."
          class="w-full pl-10 pr-9 py-2 rounded-xl text-sm bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-900 dark:text-slate-100 placeholder-slate-400 focus:outline-hidden focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-600 transition-all"
          @input="handleSearchInput"
        />
        <button
          v-if="localSearch"
          type="button"
          class="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-400 hover:text-slate-600 dark:hover:text-slate-200"
          aria-label="Borrar búsqueda"
          @click="clearSearch"
        >
          <X class="w-4 h-4" />
        </button>
      </div>

      <!-- Filtro por Categoría -->
      <div class="md:col-span-3">
        <select
          :value="selectedCategory ?? ''"
          class="w-full px-3 py-2 rounded-xl text-sm bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-800 dark:text-slate-200 focus:outline-hidden focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-600 transition-all"
          @change="handleCategoryChange"
        >
          <option value="">Todas las Categorías</option>
          <option v-for="cat in categories" :key="cat.id" :value="cat.id">
            {{ cat.name }} ({{ cat.productsCount }})
          </option>
        </select>
      </div>

      <!-- Filtro por Marca -->
      <div class="md:col-span-2">
        <select
          :value="selectedBrand ?? ''"
          class="w-full px-3 py-2 rounded-xl text-sm bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-800 dark:text-slate-200 focus:outline-hidden focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-600 transition-all"
          @change="handleBrandChange"
        >
          <option value="">Todas las Marcas</option>
          <option v-for="b in brands" :key="b.id" :value="b.id">
            {{ b.name }}
          </option>
        </select>
      </div>

      <!-- Ordenamiento -->
      <div class="md:col-span-2">
        <select
          :value="selectedOrderBy ?? ''"
          class="w-full px-3 py-2 rounded-xl text-sm bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-800 dark:text-slate-200 focus:outline-hidden focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-600 transition-all"
          @change="handleOrderChange"
        >
          <option value="">Ordenar por...</option>
          <option value="name_asc">Nombre (A - Z)</option>
          <option value="name_desc">Nombre (Z - A)</option>
          <option value="price_asc">Menor Precio</option>
          <option value="price_desc">Mayor Precio</option>
        </select>
      </div>
    </div>

    <!-- Filtros Secundarios y Botón de Limpiar -->
    <div class="flex flex-wrap items-center justify-between gap-3 pt-3 border-t border-slate-100 dark:border-slate-800/80 text-xs">
      <div class="flex items-center gap-2">
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg border font-medium transition-all inline-flex items-center gap-1.5"
          :class="
            onlyInStock
              ? 'bg-emerald-50 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300 border-emerald-300 dark:border-emerald-800 font-bold'
              : 'bg-slate-50 dark:bg-slate-800 text-slate-600 dark:text-slate-400 border-slate-200 dark:border-slate-700 hover:bg-slate-100'
          "
          @click="toggleStockFilter"
        >
          <span
            class="w-2 h-2 rounded-full"
            :class="onlyInStock ? 'bg-emerald-500' : 'bg-slate-400'"
          />
          Solo productos en stock
        </button>
      </div>

      <div class="flex items-center gap-2">
        <BaseButton
          variant="outline"
          size="sm"
          @click="emit('reset')"
        >
          <template #icon>
            <RotateCcw class="w-3.5 h-3.5" />
          </template>
          Limpiar Filtros
        </BaseButton>
      </div>
    </div>
  </div>
</template>
