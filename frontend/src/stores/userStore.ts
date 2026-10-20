import { defineStore } from 'pinia'
import type { AuthResponseDTO, UserState } from '@/types/auth'

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    email: null,
    token: null,
    role: null,
  }),

  getters: {
    /**
     * Retorna verdadero si el usuario posee un token JWT activo
     */
    isAuthenticated: (state): boolean => Boolean(state.token),

    /**
     * Retorna verdadero si el rol asignado es Administrador
     */
    isAdmin: (state): boolean => state.role?.toLowerCase() === 'admin',

    /**
     * Retorna verdadero si el rol asignado es Cliente / Kiosquero
     */
    isClient: (state): boolean => state.role?.toLowerCase() === 'client',
  },

  actions: {
    /**
     * Guarda la sesión devuelta por el microservicio de autenticación
     */
    setSession(auth: AuthResponseDTO) {
      this.email = auth.email
      this.token = auth.token
      this.role = auth.role
    },

    /**
     * Limpia la sesión actual del usuario
     */
    logout() {
      this.email = null
      this.token = null
      this.role = null
    },
  },

  persist: true, // pinia-plugin-persistedstate
})
