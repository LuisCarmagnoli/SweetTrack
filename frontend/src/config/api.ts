/**
 * Configuración de endpoints y URLs de la API para SweetTrack
 */
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_URL || '',
  ENDPOINTS: {
    AUTH: {
      LOGIN: '/api/auth/login',
      REGISTER: '/api/auth/register',
    },
  },
} as const
