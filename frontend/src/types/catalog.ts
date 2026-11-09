export interface ProductResponseDto {
  id: number
  barcode: string
  name: string
  brandId: number
  brandName: string
  categoryId: number
  categoryName: string
  wholesalePrice: number
  boxPrice: number
  unitsPerBox: number
  stockUnits: number
  safetyStock: number
  isLowStock: boolean
  imageUrl: string | null
  isActive: boolean
}

export interface ProductDetailDto extends ProductResponseDto {
  description: string | null
  createdAtUtc: string
  updatedAtUtc: string | null
  rowVersion: string | number[]
}

export interface BrandDto {
  id: number
  name: string
  logoUrl: string | null
  isActive: boolean
  productsCount: number
}

export interface CategoryDto {
  id: number
  name: string
  slug: string
  description: string | null
  isActive: boolean
  productsCount: number
}

export interface PagedResultDto<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface CatalogFilterParams {
  page?: number
  pageSize?: number
  searchTerm?: string
  categoryId?: number | null
  brandId?: number | null
  minPrice?: number | null
  maxPrice?: number | null
  onlyInStock?: boolean
  onlyActive?: boolean
  orderBy?: 'name_asc' | 'name_desc' | 'price_asc' | 'price_desc'
}

export const StockMovementType = {
  Entrada: 1,
  Salida: 2,
  AjusteInventario: 3,
} as const

export type StockMovementType = (typeof StockMovementType)[keyof typeof StockMovementType]

export interface StockAdjustmentPayload {
  quantity: number
  movementType: StockMovementType
  reason?: string
}

export const BulkImportTaskStatus = {
  EnCola: 1,
  Procesando: 2,
  Completado: 3,
  Fallido: 4,
} as const

export type BulkImportTaskStatus = (typeof BulkImportTaskStatus)[keyof typeof BulkImportTaskStatus]

export interface BulkImportResponseDto {
  taskId: string
  message: string
  status: BulkImportTaskStatus
}

export interface BulkImportStatusDto {
  taskId: string
  fileName: string
  status: BulkImportTaskStatus
  statusName: string
  totalRows: number
  processedRows: number
  updatedProductsCount: number
  createdProductsCount: number
  errorCount: number
  errorMessages: string[]
  startedAtUtc?: string | null
  finishedAtUtc?: string | null
}
