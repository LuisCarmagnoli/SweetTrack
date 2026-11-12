import { describe, it, expect } from 'vitest'
import {
  createOrderItemSchema,
  createOrderSchema,
  statusTransitionSchema,
} from '@/schemas/orderingSchemas'
import { PaymentMethod, OrderStatus } from '@/types/ordering'

describe('orderingSchemas - Validaciones de Zod para Órdenes B2B', () => {
  describe('createOrderItemSchema', () => {
    it('valida un ítem de orden correcto', () => {
      const validItem = {
        productId: 'f47ac10b-58cc-4372-a567-0e02b2c3d479',
        barcode: '7791234567890',
        productName: 'Cofler Block 300g',
        units: 24,
        boxes: 2,
        unitPrice: 3200,
      }

      const result = createOrderItemSchema.safeParse(validItem)
      expect(result.success).toBe(true)
    })

    it('falla si units es menor a 1', () => {
      const invalidItem = {
        productId: 'f47ac10b-58cc-4372-a567-0e02b2c3d479',
        barcode: '7791234567890',
        productName: 'Cofler Block 300g',
        units: 0,
        boxes: 0,
        unitPrice: 3200,
      }

      const result = createOrderItemSchema.safeParse(invalidItem)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('al menos 1 unidad')
      }
    })

    it('falla si el precio unitario es negativo o cero', () => {
      const invalidItem = {
        productId: 'f47ac10b-58cc-4372-a567-0e02b2c3d479',
        barcode: '7791234567890',
        productName: 'Cofler Block 300g',
        units: 12,
        boxes: 1,
        unitPrice: 0,
      }

      const result = createOrderItemSchema.safeParse(invalidItem)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('mayor a cero')
      }
    })
  })

  describe('createOrderSchema', () => {
    it('valida una solicitud de checkout completa', () => {
      const validOrder = {
        razonSocialKiosco: 'Kiosco El Trébol',
        cuitKiosco: '20-34567890-9',
        direccionEntrega: 'Av. Corrientes 1234, CABA',
        paymentMethod: PaymentMethod.TransferenciaBancaria,
        items: [
          {
            productId: 'f47ac10b-58cc-4372-a567-0e02b2c3d479',
            barcode: '7791234567890',
            productName: 'Cofler Block 300g',
            units: 12,
            boxes: 1,
            unitPrice: 3200,
          },
        ],
        deliveryNotes: 'Timbre 2B - Entregar entre 9 y 12 hs',
      }

      const result = createOrderSchema.safeParse(validOrder)
      expect(result.success).toBe(true)
    })

    it('falla si no contiene ítems', () => {
      const invalidOrder = {
        razonSocialKiosco: 'Kiosco El Trébol',
        cuitKiosco: '20-34567890-9',
        direccionEntrega: 'Av. Corrientes 1234, CABA',
        paymentMethod: PaymentMethod.EfectivoContraEntrega,
        items: [],
      }

      const result = createOrderSchema.safeParse(invalidOrder)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('al menos un producto')
      }
    })

    it('falla si la dirección de entrega es demasiado corta', () => {
      const invalidOrder = {
        razonSocialKiosco: 'Kiosco El Trébol',
        cuitKiosco: '20-34567890-9',
        direccionEntrega: 'Av',
        paymentMethod: PaymentMethod.CuentaCorriente,
        items: [
          {
            productId: 'f47ac10b-58cc-4372-a567-0e02b2c3d479',
            barcode: '7791234567890',
            productName: 'Cofler Block 300g',
            units: 12,
            boxes: 1,
            unitPrice: 3200,
          },
        ],
      }

      const result = createOrderSchema.safeParse(invalidOrder)
      expect(result.success).toBe(false)
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('La dirección de entrega es obligatoria')
      }
    })
  })

  describe('statusTransitionSchema', () => {
    it('valida una transición de estado correcta', () => {
      const validTransition = {
        targetStatus: OrderStatus.EnPreparacion,
        notes: 'Iniciando empaque en depósito',
      }

      const result = statusTransitionSchema.safeParse(validTransition)
      expect(result.success).toBe(true)
    })

    it('falla si el targetStatus está fuera de rango', () => {
      const invalidTransition = {
        targetStatus: 99,
        notes: 'Invalido',
      }

      const result = statusTransitionSchema.safeParse(invalidTransition)
      expect(result.success).toBe(false)
    })
  })
})
