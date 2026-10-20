import { apiClient } from './apiClient'
import { API_CONFIG } from '@/config/api'
import type {
  LoginRequestDTO,
  RegisterRequestDTO,
  AuthResponseDTO,
  RegisterResponseDTO,
} from '@/types/auth'

export const authService = {
  /**
   * Inicia sesión validando credenciales contra el microservicio SweetTrack.Auth.API
   */
  async login(credentials: LoginRequestDTO): Promise<AuthResponseDTO> {
    return await apiClient<AuthResponseDTO>(API_CONFIG.ENDPOINTS.AUTH.LOGIN, {
      method: 'POST',
      body: JSON.stringify(credentials),
    })
  },

  /**
   * Registra un nuevo usuario kiosquero (rol Client por defecto en backend)
   */
  async register(credentials: RegisterRequestDTO): Promise<RegisterResponseDTO> {
    return await apiClient<RegisterResponseDTO>(API_CONFIG.ENDPOINTS.AUTH.REGISTER, {
      method: 'POST',
      body: JSON.stringify(credentials),
    })
  },
}
