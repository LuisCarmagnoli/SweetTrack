<script setup lang="ts">
import { computed } from 'vue'
import { AlertCircle, CheckCircle2, AlertTriangle, Info, X } from '@lucide/vue'

const props = withDefaults(
  defineProps<{
    type?: 'error' | 'success' | 'warning' | 'info'
    title?: string
    message?: string
    dismissible?: boolean
  }>(),
  {
    type: 'info',
    dismissible: false,
  }
)

const emit = defineEmits<{
  (e: 'dismiss'): void
}>()

const iconComponent = computed(() => {
  switch (props.type) {
    case 'error':
      return AlertCircle
    case 'success':
      return CheckCircle2
    case 'warning':
      return AlertTriangle
    case 'info':
    default:
      return Info
  }
})

const colorClasses = computed(() => {
  switch (props.type) {
    case 'error':
      return 'bg-rose-50/90 dark:bg-rose-950/40 text-rose-800 dark:text-rose-200 border-rose-200 dark:border-rose-900/50'
    case 'success':
      return 'bg-emerald-50/90 dark:bg-emerald-950/40 text-emerald-800 dark:text-emerald-200 border-emerald-200 dark:border-emerald-900/50'
    case 'warning':
      return 'bg-amber-50/90 dark:bg-amber-950/40 text-amber-800 dark:text-amber-200 border-amber-200 dark:border-amber-900/50'
    case 'info':
    default:
      return 'bg-indigo-50/90 dark:bg-indigo-950/40 text-indigo-800 dark:text-indigo-200 border-indigo-200 dark:border-indigo-900/50'
  }
})

const iconColorClasses = computed(() => {
  switch (props.type) {
    case 'error':
      return 'text-rose-600 dark:text-rose-400'
    case 'success':
      return 'text-emerald-600 dark:text-emerald-400'
    case 'warning':
      return 'text-amber-600 dark:text-amber-400'
    case 'info':
    default:
      return 'text-indigo-600 dark:text-indigo-400'
  }
})
</script>

<template>
  <div
    :class="[
      'flex items-start gap-3 p-4 rounded-xl border text-sm animate-fade-in shadow-xs',
      colorClasses
    ]"
    role="alert"
  >
    <component :is="iconComponent" :class="['w-5 h-5 shrink-0 mt-0.5', iconColorClasses]" />
    
    <div class="flex-1 text-left">
      <h4 v-if="title" class="font-semibold mb-0.5 text-inherit">
        {{ title }}
      </h4>
      <p v-if="message" class="text-xs sm:text-sm opacity-90 leading-relaxed">
        {{ message }}
      </p>
      <slot />
    </div>

    <button
      v-if="dismissible"
      type="button"
      class="text-inherit opacity-60 hover:opacity-100 transition-opacity p-0.5"
      aria-label="Cerrar alerta"
      @click="emit('dismiss')"
    >
      <X class="w-4 h-4" />
    </button>
  </div>
</template>
