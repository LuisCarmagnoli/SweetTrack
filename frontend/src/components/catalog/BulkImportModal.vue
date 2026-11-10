<script setup lang="ts">
import { ref, watch } from 'vue'
import {
  UploadCloud,
  FileSpreadsheet,
  CheckCircle2,
  AlertCircle,
  Loader2,
} from '@lucide/vue'
import { useCatalogStore } from '@/stores/catalogStore'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'

interface Props {
  modelValue: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:modelValue', val: boolean): void
  (e: 'completed'): void
}>()

const catalogStore = useCatalogStore()

const fileInputRef = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const isDragging = ref<boolean>(false)
const errorMessage = ref<string | null>(null)

watch(
  () => catalogStore.bulkImportStatus?.status,
  (newStatus) => {
    if (newStatus === 3) {
      // Completado con éxito
      emit('completed')
    }
  }
)

function handleFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files[0]) {
    validateAndSetFile(target.files[0])
  }
}

function handleDrop(event: DragEvent) {
  isDragging.value = false
  if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
    validateAndSetFile(event.dataTransfer.files[0])
  }
}

function validateAndSetFile(file: File) {
  errorMessage.value = null
  if (
    !file.name.endsWith('.xlsx') &&
    !file.name.endsWith('.xls') &&
    file.type !== 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
  ) {
    errorMessage.value = 'Solo se admiten planillas de cálculo en formato Excel (.xlsx).'
    selectedFile.value = null
    return
  }
  selectedFile.value = file
}

async function handleUpload() {
  if (!selectedFile.value) return

  errorMessage.value = null
  try {
    await catalogStore.uploadBulkImport(selectedFile.value)
  } catch (err: any) {
    errorMessage.value = err?.message || 'Error al procesar la planilla Excel.'
  }
}

function handleClose() {
  selectedFile.value = null
  errorMessage.value = null
  catalogStore.bulkImportStatus = null
  emit('update:modelValue', false)
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    title="Importación Masiva de Precios y Stock (Excel)"
    max-width="lg"
    @update:model-value="handleClose"
  >
    <div class="space-y-4">
      <BaseAlert
        v-if="errorMessage"
        variant="error"
        :message="errorMessage"
        dismissible
        @dismiss="errorMessage = null"
      />

      <!-- Zona de Selección de Archivo (Si no está procesando o completado) -->
      <div v-if="!catalogStore.isImporting && !catalogStore.bulkImportStatus">
        <div
          class="border-2 border-dashed rounded-2xl p-6 text-center cursor-pointer transition-all"
          :class="
            isDragging
              ? 'border-indigo-500 bg-indigo-50/50 dark:bg-indigo-950/20'
              : 'border-slate-300 dark:border-slate-700 hover:border-indigo-400 bg-slate-50 dark:bg-slate-800/40'
          "
          @dragover.prevent="isDragging = true"
          @dragleave.prevent="isDragging = false"
          @drop.prevent="handleDrop"
          @click="fileInputRef?.click()"
        >
          <input
            ref="fileInputRef"
            type="file"
            accept=".xlsx, .xls"
            class="hidden"
            @change="handleFileChange"
          />

          <div class="flex flex-col items-center gap-2 text-slate-500 dark:text-slate-400">
            <div class="w-12 h-12 rounded-2xl bg-indigo-50 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-400 flex items-center justify-center mb-1">
              <UploadCloud class="w-6 h-6" />
            </div>
            <p class="font-bold text-sm text-slate-800 dark:text-slate-200">
              Haz clic para seleccionar o arrastra una planilla Excel (.xlsx)
            </p>
            <p class="text-xs">
              Debe contener columnas: Código de Barras, Nombre, Marca, Categoría, Precio Mayorista, Stock.
            </p>
          </div>
        </div>

        <!-- Archivo Seleccionado -->
        <div
          v-if="selectedFile"
          class="mt-3 p-3 rounded-xl bg-indigo-50/60 dark:bg-indigo-950/40 border border-indigo-200 dark:border-indigo-800/80 flex items-center justify-between text-xs"
        >
          <div class="flex items-center gap-2">
            <FileSpreadsheet class="w-4 h-4 text-emerald-600" />
            <span class="font-semibold text-slate-800 dark:text-slate-200">
              {{ selectedFile.name }}
            </span>
            <span class="text-slate-400">({{ (selectedFile.size / 1024).toFixed(1) }} KB)</span>
          </div>
          <button
            type="button"
            class="text-rose-500 hover:text-rose-700 font-bold"
            @click.stop="selectedFile = null"
          >
            Quitar
          </button>
        </div>
      </div>

      <!-- Estado de Procesamiento / Progreso -->
      <div v-else-if="catalogStore.isImporting" class="py-6 text-center space-y-3">
        <Loader2 class="w-8 h-8 text-indigo-600 animate-spin mx-auto" />
        <p class="font-extrabold text-base text-slate-800 dark:text-slate-200">
          Procesando planilla en segundo plano...
        </p>
        <p class="text-xs text-slate-400">
          Validando filas transaccionales y sincronizando catálogo e inventario.
        </p>
      </div>

      <!-- Resumen Final cuando terminó -->
      <div
        v-else-if="catalogStore.bulkImportStatus"
        class="p-5 rounded-2xl border text-xs space-y-3"
        :class="
          catalogStore.bulkImportStatus.status === 3
            ? 'bg-emerald-50/50 dark:bg-emerald-950/20 border-emerald-200 dark:border-emerald-800'
            : 'bg-rose-50/50 dark:bg-rose-950/20 border-rose-200 dark:border-rose-800'
        "
      >
        <div class="flex items-center gap-2 font-bold text-sm">
          <CheckCircle2
            v-if="catalogStore.bulkImportStatus.status === 3"
            class="w-5 h-5 text-emerald-600"
          />
          <AlertCircle v-else class="w-5 h-5 text-rose-600" />
          <span>
            {{ catalogStore.bulkImportStatus.status === 3 ? 'Importación Completada con Éxito' : 'Importación con Errores' }}
          </span>
        </div>

        <div class="grid grid-cols-2 sm:grid-cols-4 gap-2 text-center pt-2">
          <div class="p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800">
            <span class="text-slate-400 block text-[10px] uppercase">Filas Totales</span>
            <span class="font-extrabold text-base">{{ catalogStore.bulkImportStatus.totalRows }}</span>
          </div>
          <div class="p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800">
            <span class="text-slate-400 block text-[10px] uppercase">Actualizados</span>
            <span class="font-extrabold text-base text-indigo-600">{{ catalogStore.bulkImportStatus.updatedProductsCount }}</span>
          </div>
          <div class="p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800">
            <span class="text-slate-400 block text-[10px] uppercase">Nuevos</span>
            <span class="font-extrabold text-base text-emerald-600">{{ catalogStore.bulkImportStatus.createdProductsCount }}</span>
          </div>
          <div class="p-2 rounded-xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800">
            <span class="text-slate-400 block text-[10px] uppercase">Errores</span>
            <span class="font-extrabold text-base text-rose-600">{{ catalogStore.bulkImportStatus.errorCount }}</span>
          </div>
        </div>

        <!-- Lista de errores si hubieron -->
        <div v-if="catalogStore.bulkImportStatus.errorMessages?.length" class="mt-2 text-rose-600 dark:text-rose-400">
          <p class="font-bold mb-1">Detalles de errores:</p>
          <ul class="list-disc list-inside space-y-0.5 text-[11px] max-h-24 overflow-y-auto">
            <li v-for="(msg, i) in catalogStore.bulkImportStatus.errorMessages" :key="i">
              {{ msg }}
            </li>
          </ul>
        </div>
      </div>
    </div>

    <template #footer>
      <BaseButton
        variant="ghost"
        @click="handleClose"
      >
        Cerrar
      </BaseButton>

      <BaseButton
        v-if="!catalogStore.bulkImportStatus"
        variant="primary"
        :loading="catalogStore.isImporting"
        :disabled="!selectedFile"
        @click="handleUpload"
      >
        Iniciar Importación
      </BaseButton>
    </template>
  </BaseModal>
</template>
