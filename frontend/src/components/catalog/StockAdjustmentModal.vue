<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { ProductResponseDto, ProductDetailDto } from '@/types/catalog'
import { StockMovementType } from '@/types/catalog'
import { stockAdjustmentSchema } from '@/schemas/catalogSchemas'
import { useCatalogStore } from '@/stores/catalogStore'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'

interface Props {
  modelValue: boolean
  product: ProductResponseDto | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:modelValue', val: boolean): void
  (e: 'success', updated: ProductDetailDto): void
}>()

const catalogStore = useCatalogStore()

const quantity = ref<number>(0)
const movementType = ref<StockMovementType>(StockMovementType.AjusteInventario)
const reason = ref<string>('')
const isLoading = ref<boolean>(false)
const errorMessage = ref<string | null>(null)
const fieldErrors = ref<Record<string, string>>({})

watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) {
      quantity.value = 0
      movementType.value = StockMovementType.AjusteInventario
      reason.value = ''
      errorMessage.value = null
      fieldErrors.value = {}
    }
  }
)

const resultingStock = computed(() => {
  if (!props.product) return 0
  const qty = Number(quantity.value) || 0
  if (movementType.value === StockMovementType.Salida) {
    return Math.max(0, props.product.stockUnits - qty)
  }
  return props.product.stockUnits + qty
})

async function handleSubmit() {
  if (!props.product) return

  errorMessage.value = null
  fieldErrors.value = {}

  // Validación con Zod
  const validation = stockAdjustmentSchema.safeParse({
    quantity: Number(quantity.value),
    movementType: movementType.value,
    reason: reason.value,
  })

  if (!validation.success) {
    validation.error.issues.forEach((issue) => {
      if (issue.path[0]) {
        fieldErrors.value[issue.path[0].toString()] = issue.message
      }
    })
    return
  }

  // Prevenir egreso mayor a stock disponible
  if (
    movementType.value === StockMovementType.Salida &&
    Number(quantity.value) > props.product.stockUnits
  ) {
    errorMessage.value = `Stock insuficiente. Solo hay ${props.product.stockUnits} unidades disponibles.`
    return
  }

  isLoading.value = true
  try {
    // Si es Salida, se envía negativo al endpoint para restar
    const finalQty =
      movementType.value === StockMovementType.Salida
        ? -Math.abs(Number(quantity.value))
        : Math.abs(Number(quantity.value))

    const updated = await catalogStore.adjustStock(props.product.id, {
      quantity: finalQty,
      movementType: movementType.value,
      reason: reason.value || 'Ajuste manual de inventario desde el panel web',
    })

    emit('success', updated)
    emit('update:modelValue', false)
  } catch (err: any) {
    errorMessage.value = err?.message || 'Error al actualizar el stock.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    title="Ajuste de Stock Mayorista"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div v-if="product" class="space-y-4">
      <!-- Resumen del Producto -->
      <div class="p-3.5 rounded-xl bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700/80 text-xs">
        <p class="font-extrabold text-sm text-slate-900 dark:text-slate-100 mb-1">
          {{ product.name }}
        </p>
        <div class="flex items-center justify-between text-slate-500 dark:text-slate-400">
          <span>Marca: <strong class="text-slate-700 dark:text-slate-200">{{ product.brandName }}</strong></span>
          <span>Stock Actual: <strong class="text-indigo-600 dark:text-indigo-400 font-bold">{{ product.stockUnits }} un.</strong></span>
        </div>
      </div>

      <!-- Alerta de Error si ocurre -->
      <BaseAlert
        v-if="errorMessage"
        variant="error"
        :message="errorMessage"
        dismissible
        @dismiss="errorMessage = null"
      />

      <!-- Tipo de Movimiento -->
      <div>
        <label class="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
          Tipo de Ajuste
        </label>
        <select
          v-model="movementType"
          class="w-full px-3 py-2 rounded-xl text-sm bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-800 dark:text-slate-200 focus:outline-hidden focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-600 transition-all"
        >
          <option :value="StockMovementType.AjusteInventario">Ajuste Manual de Inventario (+ / -)</option>
          <option :value="StockMovementType.Entrada">Ingreso por Compra / Recepción de Mercadería (+)</option>
          <option :value="StockMovementType.Salida">Egreso / Rotura / Pérdida Física (-)</option>
        </select>
      </div>

      <!-- Cantidad de Unidades -->
      <div>
        <label class="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
          Cantidad de Unidades
        </label>
        <BaseInput
          v-model.number="quantity"
          type="number"
          placeholder="Ej: 50"
          :error="fieldErrors.quantity"
        />
        <p class="text-[11px] text-slate-400 mt-1">
          Stock proyectado resultante: <strong>{{ resultingStock }} unidades</strong>
        </p>
      </div>

      <!-- Motivo del Ajuste -->
      <div>
        <label class="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
          Motivo (Opcional)
        </label>
        <textarea
          v-model="reason"
          rows="2"
          placeholder="Ej: Conteo físico fin de mes, recepción de bultos dañados..."
          class="w-full px-3 py-2 rounded-xl text-sm bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-800 dark:text-slate-200 focus:outline-hidden focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-600 transition-all"
        />
      </div>
    </div>

    <template #footer>
      <BaseButton
        variant="ghost"
        :disabled="isLoading"
        @click="emit('update:modelValue', false)"
      >
        Cancelar
      </BaseButton>

      <BaseButton
        variant="primary"
        :loading="isLoading"
        @click="handleSubmit"
      >
        Confirmar Ajuste
      </BaseButton>
    </template>
  </BaseModal>
</template>
