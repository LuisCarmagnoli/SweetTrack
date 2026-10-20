import type { ApiErrorResponse } from '@/types/auth'

export class ApiError extends Error {
  public status: number
  public details: ApiErrorResponse

  constructor(status: number, message: string, details: ApiErrorResponse = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.details = details
  }
}

interface RequestOptions extends RequestInit {
  token?: string | null
}

export async function apiClient<T>(
  endpoint: string,
  options: RequestOptions = {}
): Promise<T> {
  const { token, headers = {}, ...customConfig } = options

  const defaultHeaders: HeadersInit = {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  }

  if (token) {
    (defaultHeaders as Record<string, string>)['Authorization'] = `Bearer ${token}`
  }

  const config: RequestInit = {
    ...customConfig,
    headers: {
      ...defaultHeaders,
      ...headers,
    },
  }

  try {
    const response = await fetch(endpoint, config)

    // Si la respuesta no es 2xx
    if (!response.ok) {
      let errorData: ApiErrorResponse = {}
      try {
        errorData = await response.json()
      } catch {
        errorData = { message: response.statusText || 'Error inesperado de red.' }
      }

      // Mensaje priorizando message o title (RFC 7807)
      const errorMessage =
        errorData.message ||
        errorData.title ||
        (response.status === 401
          ? 'Credenciales incorrectas o sesión expirada.'
          : response.status === 400
          ? 'Los datos enviados son inválidos.'
          : 'Ocurrió un error al procesar la solicitud.')

      throw new ApiError(response.status, errorMessage, errorData)
    }

    // 204 No Content
    if (response.status === 204) {
      return {} as T
    }

    return await response.json()
  } catch (error) {
    if (error instanceof ApiError) {
      throw error
    }
    throw new ApiError(0, 'No se pudo conectar con el servidor. Revisa tu conexión de red.')
  }
}
