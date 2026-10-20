import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useUserStore } from '@/stores/userStore'

describe('useUserStore - Gestión de Sesión', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('debe inicializarse en estado no autenticado', () => {
    const store = useUserStore()
    expect(store.token).toBeNull()
    expect(store.email).toBeNull()
    expect(store.role).toBeNull()
    expect(store.isAuthenticated).toBe(false)
  })

  it('debe actualizar los datos de sesión correctamente con setSession', () => {
    const store = useUserStore()
    store.setSession({
      email: 'kiosco@sweettrack.com',
      token: 'jwt.mock.token',
      role: 'Client',
    })

    expect(store.email).toBe('kiosco@sweettrack.com')
    expect(store.token).toBe('jwt.mock.token')
    expect(store.role).toBe('Client')
    expect(store.isAuthenticated).toBe(true)
    expect(store.isClient).toBe(true)
    expect(store.isAdmin).toBe(false)
  })

  it('debe reconocer rol Admin correctamente', () => {
    const store = useUserStore()
    store.setSession({
      email: 'admin@sweettrack.com',
      token: 'jwt.admin.token',
      role: 'Admin',
    })

    expect(store.isAdmin).toBe(true)
    expect(store.isClient).toBe(false)
  })

  it('debe limpiar los datos de sesión al invocar logout', () => {
    const store = useUserStore()
    store.setSession({
      email: 'kiosco@sweettrack.com',
      token: 'jwt.mock.token',
      role: 'Client',
    })

    expect(store.isAuthenticated).toBe(true)

    store.logout()

    expect(store.email).toBeNull()
    expect(store.token).toBeNull()
    expect(store.role).toBeNull()
    expect(store.isAuthenticated).toBe(false)
  })
})
