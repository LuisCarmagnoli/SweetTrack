<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Mail, Lock, ArrowRight, UserPlus } from '@lucide/vue'
import AuthCard from '@/components/auth/AuthCard.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseAlert from '@/components/base/BaseAlert.vue'
import { authService } from '@/services/authService'
import { ApiError } from '@/services/apiClient'
import { useUserStore } from '@/stores/userStore'
import { loginSchema, registerSchema } from '@/schemas/authSchemas'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

// Pestaña activa ('login' | 'register')
const activeTab = ref<'login' | 'register'>('login')

// Estado del formulario de Login
const loginEmail = ref('')
const loginPassword = ref('')
const loginErrors = ref<{ email?: string; password?: string }>({})
const loginLoading = ref(false)
const loginAlert = ref<{ type: 'error' | 'success'; message: string } | null>(null)

// Estado del formulario de Registro
const registerEmail = ref('')
const registerPassword = ref('')
const registerConfirmPassword = ref('')
const registerErrors = ref<{ email?: string; password?: string; confirmPassword?: string }>({})
const registerLoading = ref(false)
const registerAlert = ref<{ type: 'error' | 'success'; message: string } | null>(null)

/**
 * Procesa el inicio de sesión
 */
async function handleLogin() {
  loginAlert.value = null
  loginErrors.value = {}

  // Validación con Zod
  const validation = loginSchema.safeParse({
    email: loginEmail.value,
    password: loginPassword.value,
  })

  if (!validation.success) {
    const formatted = validation.error.format()
    loginErrors.value = {
      email: formatted.email?._errors[0],
      password: formatted.password?._errors[0],
    }
    return
  }

  loginLoading.value = true

  try {
    const response = await authService.login({
      email: loginEmail.value.trim(),
      password: loginPassword.value,
    })

    // Guardar sesión en store persistido
    userStore.setSession(response)

    // Redirigir al dashboard o ruta solicitada previa
    const redirectPath = (route.query.redirect as string) || '/dashboard'
    await router.push(redirectPath)
  } catch (error) {
    if (error instanceof ApiError) {
      loginAlert.value = {
        type: 'error',
        message: error.message,
      }
    } else {
      loginAlert.value = {
        type: 'error',
        message: 'No fue posible iniciar sesión. Verifica tu conexión.',
      }
    }
  } finally {
    loginLoading.value = false
  }
}

/**
 * Procesa el registro de un nuevo kiosquero
 */
async function handleRegister() {
  registerAlert.value = null
  registerErrors.value = {}

  // Validación con Zod
  const validation = registerSchema.safeParse({
    email: registerEmail.value,
    password: registerPassword.value,
    confirmPassword: registerConfirmPassword.value,
  })

  if (!validation.success) {
    const formatted = validation.error.format()
    registerErrors.value = {
      email: formatted.email?._errors[0],
      password: formatted.password?._errors[0],
      confirmPassword: formatted.confirmPassword?._errors[0],
    }
    return
  }

  registerLoading.value = true

  try {
    const response = await authService.register({
      email: registerEmail.value.trim(),
      password: registerPassword.value,
    })

    // Registro exitoso: cambiar a login y notificar
    activeTab.value = 'login'
    loginEmail.value = registerEmail.value
    loginPassword.value = ''
    loginAlert.value = {
      type: 'success',
      message: response.message || 'Kiosco registrado exitosamente. Ya puedes iniciar sesión.',
    }

    // Reset campos de registro
    registerEmail.value = ''
    registerPassword.value = ''
    registerConfirmPassword.value = ''
  } catch (error) {
    if (error instanceof ApiError) {
      registerAlert.value = {
        type: 'error',
        message: error.message,
      }
    } else {
      registerAlert.value = {
        type: 'error',
        message: 'No fue posible completar el registro. Inténtalo nuevamente.',
      }
    }
  } finally {
    registerLoading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4 sm:p-6 bg-gradient-to-br from-slate-100 via-indigo-50/40 to-slate-100 dark:from-slate-950 dark:via-slate-900 dark:to-slate-950">
    <AuthCard :active-tab="activeTab" @update:active-tab="activeTab = $event">
      <!-- ============================================== -->
      <!-- PESTAÑA 1: INICIAR SESIÓN -->
      <!-- ============================================== -->
      <form v-if="activeTab === 'login'" class="flex flex-col gap-4" @submit.prevent="handleLogin">
        <!-- Alerta de Error o Éxito -->
        <BaseAlert
          v-if="loginAlert"
          :type="loginAlert.type"
          :message="loginAlert.message"
          dismissible
          @dismiss="loginAlert = null"
        />

        <!-- Input Email -->
        <BaseInput
          id="login-email"
          v-model="loginEmail"
          label="Correo Electrónico"
          type="email"
          placeholder="kiosco@ejemplo.com"
          :error="loginErrors.email"
          required
          autocomplete="username"
          :disabled="loginLoading"
        >
          <template #icon>
            <Mail class="w-4 h-4" />
          </template>
        </BaseInput>

        <!-- Input Password -->
        <BaseInput
          id="login-password"
          v-model="loginPassword"
          label="Contraseña"
          type="password"
          placeholder="••••••••"
          :error="loginErrors.password"
          required
          autocomplete="current-password"
          :disabled="loginLoading"
        >
          <template #icon>
            <Lock class="w-4 h-4" />
          </template>
        </BaseInput>

        <!-- Botón Submit -->
        <div class="mt-2">
          <BaseButton
            type="submit"
            variant="primary"
            block
            :loading="loginLoading"
          >
            <template #icon>
              <ArrowRight class="w-4 h-4" />
            </template>
            Ingresar al Portal
          </BaseButton>
        </div>
      </form>

      <!-- ============================================== -->
      <!-- PESTAÑA 2: REGISTRAR KIOSCO -->
      <!-- ============================================== -->
      <form v-else class="flex flex-col gap-4" @submit.prevent="handleRegister">
        <!-- Alerta de Registro -->
        <BaseAlert
          v-if="registerAlert"
          :type="registerAlert.type"
          :message="registerAlert.message"
          dismissible
          @dismiss="registerAlert = null"
        />

        <!-- Input Email -->
        <BaseInput
          id="register-email"
          v-model="registerEmail"
          label="Correo Electrónico del Kiosco"
          type="email"
          placeholder="miminimercado@ejemplo.com"
          :error="registerErrors.email"
          required
          autocomplete="email"
          :disabled="registerLoading"
        >
          <template #icon>
            <Mail class="w-4 h-4" />
          </template>
        </BaseInput>

        <!-- Input Password -->
        <BaseInput
          id="register-password"
          v-model="registerPassword"
          label="Contraseña (Mínimo 6 caracteres)"
          type="password"
          placeholder="••••••••"
          :error="registerErrors.password"
          required
          autocomplete="new-password"
          :disabled="registerLoading"
        >
          <template #icon>
            <Lock class="w-4 h-4" />
          </template>
        </BaseInput>

        <!-- Input Confirm Password -->
        <BaseInput
          id="register-confirm-password"
          v-model="registerConfirmPassword"
          label="Confirmar Contraseña"
          type="password"
          placeholder="••••••••"
          :error="registerErrors.confirmPassword"
          required
          autocomplete="new-password"
          :disabled="registerLoading"
        >
          <template #icon>
            <Lock class="w-4 h-4" />
          </template>
        </BaseInput>

        <!-- Botón Submit Registro -->
        <div class="mt-2">
          <BaseButton
            type="submit"
            variant="secondary"
            block
            :loading="registerLoading"
          >
            <template #icon>
              <UserPlus class="w-4 h-4" />
            </template>
            Crear Cuenta de Kiosco
          </BaseButton>
        </div>
      </form>
    </AuthCard>
  </div>
</template>
