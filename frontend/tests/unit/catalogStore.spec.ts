import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCatalogStore } from '@/stores/catalogStore'
import { catalogService } from '@/services/catalogService'
import type { ProductResponseDto, PagedResultDto, ProductDetailDto } from '@/types/catalog'
import { StockMovementType } from '@/types/catalog'

vi.mock('@/services/catalogService', () => ({
  catalogService: {
    getProducts: vi.fn(),
    getCategories: vi.fn(),
    getBrands: vi.fn(),
    adjustStock: vi.fn(),
    uploadBulkImport: vi.fn(),
    getBulkImportStatus: vi.fn(),
  },
}))

describe('useCatalogStore - Estado y Gestión de Catálogo', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  const mockProduct: ProductResponseDto = {
    id: 1,
    barcode: '77900401',
    name: 'Bon o Bon Leche x 18 un.',
    brandId: 1,
    brandName: 'Arcor',
    categoryId: 1,
    categoryName: 'Chocolates',
    wholesalePrice: 4500,
    boxPrice: 54000,
    unitsPerBox: 12,
    stockUnits: 150,
    safetyStock: 20,
    isLowStock: false,
    imageUrl: null,
    isActive: true,
  }

  const mockPagedResult: PagedResultDto<ProductResponseDto> = {
    items: [mockProduct],
    page: 1,
    pageSize: 12,
    totalCount: 1,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  }

  it('debe inicializarse con estado por defecto', () => {
    const store = useCatalogStore()
    expect(store.products).toEqual([])
    expect(store.totalCount).toBe(0)
    expect(store.page).toBe(1)
    expect(store.isLoading).toBe(false)
    expect(store.hasProducts).toBe(false)
  })

  it('debe cargar productos y actualizar estado con fetchProducts', async () => {
    vi.mocked(catalogService.getProducts).mockResolvedValue(mockPagedResult)

    const store = useCatalogStore()
    await store.fetchProducts(1)

    expect(catalogService.getProducts).toHaveBeenCalledTimes(1)
    expect(store.products).toHaveLength(1)
    expect(store.products[0].name).toBe('Bon o Bon Leche x 18 un.')
    expect(store.totalCount).toBe(1)
    expect(store.hasProducts).toBe(true)
    expect(store.isLoading).toBe(false)
  })

  it('debe actualizar el filtro de búsqueda y disparar recarga', async () => {
    vi.mocked(catalogService.getProducts).mockResolvedValue(mockPagedResult)

    const store = useCatalogStore()
    store.setSearchTerm('Oreo')

    expect(store.filters.searchTerm).toBe('Oreo')
    expect(store.filters.page).toBe(1)
    expect(catalogService.getProducts).toHaveBeenCalled()
  })

  it('debe actualizar categoría y marca correctamente', async () => {
    vi.mocked(catalogService.getProducts).mockResolvedValue(mockPagedResult)

    const store = useCatalogStore()
    store.setCategory(2)
    store.setBrand(3)

    expect(store.filters.categoryId).toBe(2)
    expect(store.filters.brandId).toBe(3)
  })

  it('debe restablecer los filtros a su estado inicial', async () => {
    vi.mocked(catalogService.getProducts).mockResolvedValue(mockPagedResult)

    const store = useCatalogStore()
    store.setSearchTerm('Alfajor')
    store.setCategory(5)

    store.resetFilters()

    expect(store.filters.searchTerm).toBe('')
    expect(store.filters.categoryId).toBeNull()
    expect(store.filters.brandId).toBeNull()
  })

  it('debe actualizar el stock local tras un ajuste exitoso', async () => {
    const updatedDetail: ProductDetailDto = {
      ...mockProduct,
      stockUnits: 180,
      description: null,
      createdAtUtc: new Date().toISOString(),
      updatedAtUtc: null,
      rowVersion: 'mock',
    }

    vi.mocked(catalogService.adjustStock).mockResolvedValue(updatedDetail)

    const store = useCatalogStore()
    store.products = [{ ...mockProduct }]

    await store.adjustStock(1, {
      quantity: 30,
      movementType: StockMovementType.Entrada,
      reason: 'Recepción mercadería',
    })

    expect(catalogService.adjustStock).toHaveBeenCalledWith(1, {
      quantity: 30,
      movementType: StockMovementType.Entrada,
      reason: 'Recepción mercadería',
    }, null)

    expect(store.products[0].stockUnits).toBe(180)
  })
})
