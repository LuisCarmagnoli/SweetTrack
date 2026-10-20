/**
 * DTOs y tipos de autenticación para SweetTrack Auth API
 */

export interface LoginRequestDTO {
  email: string
  password: string
}

export interface RegisterRequestDTO {
  email: string
  password: string
}

export interface AuthResponseDTO {
  email: string
  token: string
  role: string
}

export interface RegisterResponseDTO {
  message: string
}

export interface ApiErrorResponse {
  message?: string
  title?: string
  status?: number
  errors?: Record<string, string[]>
}

export interface UserState {
  email: string | null
  token: string | null
  role: string | null
}
