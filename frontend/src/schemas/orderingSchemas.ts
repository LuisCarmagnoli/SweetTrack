import { z } from 'zod'

export const createOrderItemSchema = z.object({
  productId: z.string().min(1, 'El identificador de producto es requerido'),
  barcode: z.string().min(1, 'El código de barras es requerido'),
  productName: z.string().min(1, 'El nombre del producto es requerido'),
  units: z.number().int().min(1, 'Debe solicitar al menos 1 unidad'),
  boxes: z.number().int().min(0, 'La cantidad de bultos no puede ser negativa'),
  unitPrice: z.number().positive('El precio unitario debe ser mayor a cero'),
})

export const createOrderSchema = z.object({
  razonSocialKiosco: z.string().min(3, 'La razón social debe tener al menos 3 caracteres').max(150),
  cuitKiosco: z.string().min(11, 'El CUIT debe tener al menos 11 dígitos').max(20),
  direccionEntrega: z.string().min(5, 'La dirección de entrega es obligatoria').max(250),
  paymentMethod: z.union([z.literal(1), z.literal(2), z.literal(3)], {
    errorMap: () => ({ message: 'Seleccione un método de pago válido' }),
  }),
  items: z.array(createOrderItemSchema).min(1, 'El carrito debe contener al menos un producto'),
  deliveryNotes: z.string().max(500, 'Las notas no pueden superar 500 caracteres').optional().nullable(),
})

export type CreateOrderFormValues = z.infer<typeof createOrderSchema>

export const statusTransitionSchema = z.object({
  targetStatus: z.number().int().min(1).max(7),
  notes: z.string().max(500).optional().nullable(),
})

export type StatusTransitionFormValues = z.infer<typeof statusTransitionSchema>
