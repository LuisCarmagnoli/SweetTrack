import { z } from 'zod'
import { StockMovementType } from '@/types/catalog'

export const stockAdjustmentSchema = z.object({
  quantity: z
    .number({ invalid_type_error: 'La cantidad debe ser un número entero' })
    .int('La cantidad debe ser entera')
    .refine((val) => val !== 0, 'La cantidad a ajustar no puede ser cero'),
  movementType: z.union([
    z.literal(StockMovementType.Entrada),
    z.literal(StockMovementType.Salida),
    z.literal(StockMovementType.AjusteInventario),
  ], {
    errorMap: () => ({ message: 'Tipo de movimiento inválido' }),
  }),
  reason: z
    .string()
    .max(200, 'El motivo no puede exceder los 200 caracteres')
    .optional()
    .or(z.literal('')),
})

export type StockAdjustmentForm = z.infer<typeof stockAdjustmentSchema>

export const catalogFilterSchema = z.object({
  page: z.number().int().min(1).default(1),
  pageSize: z.number().int().min(1).max(100).default(20),
  searchTerm: z.string().optional(),
  categoryId: z.number().int().positive().nullable().optional(),
  brandId: z.number().int().positive().nullable().optional(),
  minPrice: z.number().min(0).nullable().optional(),
  maxPrice: z.number().min(0).nullable().optional(),
  onlyInStock: z.boolean().optional(),
  orderBy: z.enum(['name_asc', 'name_desc', 'price_asc', 'price_desc']).optional(),
})

export type CatalogFilterForm = z.infer<typeof catalogFilterSchema>
