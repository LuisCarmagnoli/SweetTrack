<script setup lang="ts">
import { computed } from 'vue'
import { Loader2 } from '@lucide/vue'

const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'outline' | 'ghost'
    type?: 'button' | 'submit' | 'reset'
    loading?: boolean
    disabled?: boolean
    block?: boolean
  }>(),
  {
    variant: 'primary',
    type: 'button',
    loading: false,
    disabled: false,
    block: false,
  }
)

const emit = defineEmits<{
  (e: 'click', event: MouseEvent): void
}>()

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'secondary':
      return 'bg-amber-500 hover:bg-amber-600 text-white shadow-amber-500/25'
    case 'outline':
      return 'border border-slate-300 dark:border-slate-700 hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-700 dark:text-slate-200'
    case 'ghost':
      return 'hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-300'
    case 'primary':
    default:
      return 'bg-indigo-600 hover:bg-indigo-700 text-white shadow-indigo-600/25'
  }
})
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'inline-flex items-center justify-center gap-2 font-semibold text-sm rounded-xl py-3 px-5 transition-all duration-200 cursor-pointer shadow-sm active:scale-[0.98] outline-hidden',
      variantClasses,
      block ? 'w-full' : '',
      disabled || loading ? 'opacity-65 cursor-not-allowed pointer-events-none' : '',
    ]"
    @click="emit('click', $event)"
  >
    <Loader2 v-if="loading" class="w-4 h-4 animate-spin shrink-0" />
    <slot v-if="!loading" name="icon" />
    <span>
      <slot />
    </span>
  </button>
</template>
