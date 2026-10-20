<script setup lang="ts">
import { ref, computed } from 'vue'
import { Eye, EyeOff } from '@lucide/vue'

const props = withDefaults(
  defineProps<{
    modelValue: string
    label?: string
    type?: string
    placeholder?: string
    error?: string
    id?: string
    required?: boolean
    disabled?: boolean
    autocomplete?: string
  }>(),
  {
    type: 'text',
    placeholder: '',
    error: '',
    required: false,
    disabled: false,
    autocomplete: 'off',
  }
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
  (e: 'blur'): void
}>()

const isPasswordVisible = ref(false)

const computedType = computed(() => {
  if (props.type === 'password') {
    return isPasswordVisible.value ? 'text' : 'password'
  }
  return props.type
})

const inputId = computed(() => props.id || `input-${Math.random().toString(36).substring(2, 9)}`)

function togglePassword() {
  isPasswordVisible.value = !isPasswordVisible.value
}
</script>

<template>
  <div class="w-full flex flex-col gap-1.5 text-left">
    <label
      v-if="label"
      :for="inputId"
      class="text-xs font-semibold uppercase tracking-wider text-slate-700 dark:text-slate-300"
    >
      {{ label }}
      <span v-if="required" class="text-rose-500">*</span>
    </label>

    <div class="relative flex items-center">
      <!-- Icono prefijo opcional -->
      <div
        v-if="$slots.icon"
        class="absolute left-3.5 flex items-center pointer-events-none text-slate-400 dark:text-slate-500"
      >
        <slot name="icon" />
      </div>

      <input
        :id="inputId"
        :type="computedType"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :autocomplete="autocomplete"
        :class="[
          'w-full rounded-xl border bg-white dark:bg-slate-900 py-3 text-sm text-slate-900 dark:text-slate-100 shadow-xs transition-all duration-200 outline-hidden',
          $slots.icon ? 'pl-10' : 'pl-3.5',
          type === 'password' ? 'pr-11' : 'pr-3.5',
          error
            ? 'border-rose-400 dark:border-rose-500/80 focus:border-rose-500 focus:ring-3 focus:ring-rose-500/15'
            : 'border-slate-200 dark:border-slate-800 focus:border-indigo-600 dark:focus:border-indigo-500 focus:ring-3 focus:ring-indigo-500/15',
          disabled ? 'opacity-60 cursor-not-allowed bg-slate-100 dark:bg-slate-800' : ''
        ]"
        @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
        @blur="emit('blur')"
      />

      <!-- Botón para alternar contraseña -->
      <button
        v-if="type === 'password'"
        type="button"
        tabindex="-1"
        :aria-label="isPasswordVisible ? 'Ocultar contraseña' : 'Ver contraseña'"
        class="absolute right-3.5 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 transition-colors cursor-pointer"
        @click="togglePassword"
      >
        <EyeOff v-if="isPasswordVisible" class="w-4 h-4" />
        <Eye v-else class="w-4 h-4" />
      </button>
    </div>

    <!-- Mensaje de error -->
    <p
      v-if="error"
      class="text-xs font-medium text-rose-600 dark:text-rose-400 flex items-center gap-1 mt-0.5 animate-fade-in"
    >
      {{ error }}
    </p>
  </div>
</template>
