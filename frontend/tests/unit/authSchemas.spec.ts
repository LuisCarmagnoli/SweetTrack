import { describe, it, expect } from 'vitest'
import { loginSchema, registerSchema } from '@/schemas/authSchemas'

describe('authSchemas - Validaciones Zod', () => {
  describe('loginSchema', () => {
    it('debe validar exitosamente credenciales correctas', () => {
      const validData = {
        email: 'kiosco@ejemplo.com',
        password: 'Password123!',
      }
      const result = loginSchema.safeParse(validData)
      expect(result.success).toBe(true)
    })

    it('debe fallar si el email está vacío', () => {
      const invalidData = {
        email: '',
        password: 'Password123!',
      }
      const result = loginSchema.safeParse(invalidData)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0]?.message).toContain('correo electrónico es requerido')
      }
    })

    it('debe fallar si el formato del email es inválido', () => {
      const invalidData = {
        email: 'correo-invalido',
        password: 'Password123!',
      }
      const result = loginSchema.safeParse(invalidData)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0]?.message).toContain('correo electrónico válido')
      }
    })

    it('debe fallar si la contraseña está vacía', () => {
      const invalidData = {
        email: 'kiosco@ejemplo.com',
        password: '',
      }
      const result = loginSchema.safeParse(invalidData)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0]?.message).toContain('contraseña es requerida')
      }
    })
  })

  describe('registerSchema', () => {
    it('debe validar exitosamente datos válidos de registro', () => {
      const validData = {
        email: 'kiosco@ejemplo.com',
        password: 'secret123',
        confirmPassword: 'secret123',
      }
      const result = registerSchema.safeParse(validData)
      expect(result.success).toBe(true)
    })

    it('debe fallar si la contraseña tiene menos de 6 caracteres', () => {
      const invalidData = {
        email: 'kiosco@ejemplo.com',
        password: '12345',
        confirmPassword: '12345',
      }
      const result = registerSchema.safeParse(invalidData)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0]?.message).toContain('al menos 6 caracteres')
      }
    })

    it('debe fallar si las contraseñas no coinciden', () => {
      const invalidData = {
        email: 'kiosco@ejemplo.com',
        password: 'password123',
        confirmPassword: 'different123',
      }
      const result = registerSchema.safeParse(invalidData)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0]?.message).toContain('Las contraseñas no coinciden')
      }
    })
  })
})
