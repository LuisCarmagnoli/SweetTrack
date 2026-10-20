import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import LoginView from '@/views/auth/LoginView.vue'

describe('LoginView - Interacción y Renderizado', () => {
  let router: ReturnType<typeof createRouter>

  beforeEach(async () => {
    setActivePinia(createPinia())

    router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/login', component: LoginView },
        { path: '/dashboard', component: { template: '<div>Dashboard</div>' } },
      ],
    })

    router.push('/login')
    await router.isReady()
  })

  it('debe renderizar el título de la plataforma SweetTrack y el selector de pestañas', () => {
    const wrapper = mount(LoginView, {
      global: {
        plugins: [router],
      },
    })

    expect(wrapper.text()).toContain('SweetTrack')
    expect(wrapper.text()).toContain('B2B')
    expect(wrapper.text()).toContain('Iniciar Sesión')
    expect(wrapper.text()).toContain('Registrar Kiosco')
  })

  it('debe mostrar los inputs de Login por defecto', () => {
    const wrapper = mount(LoginView, {
      global: {
        plugins: [router],
      },
    })

    const emailInput = wrapper.find('#login-email')
    const passwordInput = wrapper.find('#login-password')

    expect(emailInput.exists()).toBe(true)
    expect(passwordInput.exists()).toBe(true)
    expect(wrapper.find('button[type="submit"]').text()).toContain('Ingresar al Portal')
  })

  it('debe cambiar a los inputs de registro al hacer clic en Registrar Kiosco', async () => {
    const wrapper = mount(LoginView, {
      global: {
        plugins: [router],
      },
    })

    // Click en la pestaña "Registrar Kiosco"
    const tabs = wrapper.findAll('button[type="button"]')
    const registerTab = tabs.find((t) => t.text().includes('Registrar Kiosco'))
    expect(registerTab).toBeDefined()
    await registerTab?.trigger('click')

    expect(wrapper.find('#register-email').exists()).toBe(true)
    expect(wrapper.find('#register-password').exists()).toBe(true)
    expect(wrapper.find('#register-confirm-password').exists()).toBe(true)
    expect(wrapper.find('button[type="submit"]').text()).toContain('Crear Cuenta de Kiosco')
  })

  it('debe mostrar errores de validación si se envía el formulario vacío', async () => {
    const wrapper = mount(LoginView, {
      global: {
        plugins: [router],
      },
    })

    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.text()).toContain('El correo electrónico es requerido')
    expect(wrapper.text()).toContain('La contraseña es requerida')
  })
})
