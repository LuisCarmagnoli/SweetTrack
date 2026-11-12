<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cartStore'
import { useOrderStore } from '@/stores/orderStore'
import { createOrderSchema } from '@/schemas/orderingSchemas'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'
import { PaymentMethod, type CreateOrderItemDto } from '@/types/ordering'

interface Props {
  isOpen: boolean
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'close'): void
  (e: 'success', orderId: string): void
}>()

const router = useRouter()
const cartStore = useCartStore()
const orderStore = useOrderStore()

const form = ref({
  razonSocialKiosco: 'Kiosco Dulce Tentación',
  cuitKiosco: '20-33445566-7',
  direccionEntrega: 'Av. Corrientes 3200, CABA',
  paymentMethod: PaymentMethod.CuentaCorriente,
  deliveryNotes: '',
})

const errorMessage = ref<string | null>(null)
const fieldErrors = ref<Record<string, string>>({})
const isSubmitting = ref(false)

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 2,
  }).format(amount)
}

async function handleSubmitOrder() {
  errorMessage.value = null
  fieldErrors.value = {}

  // Mapear ítems del carrito a DTO
  const itemsDto: CreateOrderItemDto[] = cartStore.items.map(item => ({
    productId: String(item.productId),
    barcode: item.barcode,
    productName: item.productName,
    units: item.units,
    boxes: item.boxes,
    unitPrice: item.wholesalePrice,
  }))

  const payload = {
    razonSocialKiosco: form.value.razonSocialKiosco,
    cuitKiosco: form.value.cuitKiosco,
    direccionEntrega: form.value.direccionEntrega,
    paymentMethod: Number(form.value.paymentMethod) as PaymentMethod,
    items: itemsDto,
    deliveryNotes: form.value.deliveryNotes || null,
  }

  // Validación con Zod
  const validation = createOrderSchema.safeParse(payload)
  if (!validation.success) {
    const formatted = validation.error.format()
    fieldErrors.value = {
      razonSocialKiosco: formatted.razonSocialKiosco?._errors[0] || '',
      cuitKiosco: formatted.cuitKiosco?._errors[0] || '',
      direccionEntrega: formatted.direccionEntrega?._errors[0] || '',
    }
    errorMessage.value = 'Por favor, revisa los datos del formulario.'
    return
  }

  isSubmitting.value = true

  try {
    const createdOrder = await orderStore.createOrder(payload)
    cartStore.clearCart()
    emit('close')
    emit('success', createdOrder.id)
    router.push('/my-orders')
  } catch (err: any) {
    errorMessage.value = err.message || 'Ocurrió un error al confirmar tu pedido.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="isOpen"
    title="Confirmar Pedido Mayorista"
    @update:model-value="emit('close')"
    @close="emit('close')"
  >
    <div class="space-y-4">
      <BaseAlert v-if="errorMessage" type="error" :message="errorMessage" />

      <!-- Resumen de Totales -->
      <div class="p-4 rounded-lg bg-brand-50 dark:bg-brand-950/40 border border-brand-200 dark:border-brand-800 text-sm">
        <div class="flex justify-between items-center text-gray-700 dark:text-gray-300">
          <span>Total de Bultos / Unidades:</span>
          <span class="font-bold">{{ cartStore.totalBoxes }} bultos ({{ cartStore.totalUnits }} u.)</span>
        </div>
        <div class="flex justify-between items-center text-base font-extrabold text-brand-900 dark:text-brand-100 mt-2 pt-2 border-t border-brand-200 dark:border-brand-800">
          <span>Importe Total Final (IVA inc.):</span>
          <span class="text-lg text-brand-600 dark:text-brand-400">
            {{ formatCurrency(cartStore.totalAmount) }}
          </span>
        </div>
      </div>

      <!-- Datos del Kiosco -->
      <div class="space-y-3">
        <BaseInput
          v-model="form.razonSocialKiosco"
          label="Razón Social / Nombre del Kiosco"
          placeholder="Ej: Kiosco Central"
          :error="fieldErrors.razonSocialKiosco"
          required
        />

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <BaseInput
            v-model="form.cuitKiosco"
            label="CUIT"
            placeholder="20-12345678-9"
            :error="fieldErrors.cuitKiosco"
            required
          />

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Método de Pago
            </label>
            <select
              v-model.number="form.paymentMethod"
              class="w-full px-3 py-2 text-sm rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-brand-500"
            >
              <option :value="PaymentMethod.CuentaCorriente">Cuenta Corriente (Mayorista)</option>
              <option :value="PaymentMethod.TransferenciaBancaria">Transferencia Bancaria</option>
              <option :value="PaymentMethod.EfectivoContraEntrega">Efectivo contra Entrega</option>
            </select>
          </div>
        </div>

        <BaseInput
          v-model="form.direccionEntrega"
          label="Dirección de Entrega"
          placeholder="Ej: Av. Rivadavia 2500, Local 3"
          :error="fieldErrors.direccionEntrega"
          required
        />

        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Instrucciones o Notas de Despacho (Opcional)
          </label>
          <textarea
            v-model="form.deliveryNotes"
            rows="2"
            maxlength="500"
            placeholder="Ej: Recibir entre las 14 y 18 hs. Timbre blanco."
            class="w-full px-3 py-2 text-sm rounded-lg border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 focus:ring-2 focus:ring-brand-500"
          />
        </div>
      </div>
    </div>

    <template #footer>
      <div class="flex justify-end gap-3">
        <BaseButton variant="outline" :disabled="isSubmitting" @click="emit('close')">
          Cancelar
        </BaseButton>
        <BaseButton :disabled="isSubmitting || !cartStore.isMinimumMet" @click="handleSubmitOrder">
          <span v-if="isSubmitting">Procesando Pedido...</span>
          <span v-else>Emitir Pedido Definitivo</span>
        </BaseButton>
      </div>
    </template>
  </BaseModal>
</template>
