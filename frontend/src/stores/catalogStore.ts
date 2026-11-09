import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { catalogService } from '@/services/catalogService'
import { useUserStore } from '@/stores/userStore'
import type {
  ProductResponseDto,
  ProductDetailDto,
  CategoryDto,
  BrandDto,
  CatalogFilterParams,
  StockAdjustmentPayload,
  BulkImportStatusDto,
} from '@/types/catalog'
import { ApiError } from '@/services/apiClient'

export const useCatalogStore = defineStore('catalog', () => {
  const userStore = useUserStore()

  // Estado
  const products = ref<ProductResponseDto[]>([])
  const totalCount = ref<number>(0)
  const page = ref<number>(1)
  const pageSize = ref<number>(12)
  const totalPages = ref<number>(1)
  const hasPreviousPage = ref<boolean>(false)
  const hasNextPage = ref<boolean>(false)

  const categories = ref<CategoryDto[]>([])
  const brands = ref<BrandDto[]>([])

  const filters = ref<CatalogFilterParams>({
    page: 1,
    pageSize: 12,
    searchTerm: '',
    categoryId: null,
    brandId: null,
    onlyInStock: undefined,
    orderBy: undefined,
  })

  const isLoading = ref<boolean>(false)
  const isMetadataLoading = ref<boolean>(false)
  const errorMessage = ref<string | null>(null)

  const selectedProduct = ref<ProductDetailDto | null>(null)
  const bulkImportStatus = ref<BulkImportStatusDto | null>(null)
  const isImporting = ref<boolean>(false)

  // Getters
  const hasProducts = computed(() => products.value.length > 0)
  const lowStockCount = computed(() => products.value.filter((p) => p.isLowStock).length)
  const outOfStockCount = computed(() => products.value.filter((p) => p.stockUnits === 0).length)

  // Acciones
  async function fetchProducts(targetPage?: number) {
    if (targetPage !== undefined) {
      filters.value.page = targetPage
      page.value = targetPage
    }

    isLoading.value = true
    errorMessage.value = null

    try {
      const response = await catalogService.getProducts(filters.value, userStore.token)
      products.value = response.items
      totalCount.value = response.totalCount
      page.value = response.page
      pageSize.value = response.pageSize
      totalPages.value = response.totalPages
      hasPreviousPage.value = response.hasPreviousPage
      hasNextPage.value = response.hasNextPage
    } catch (err) {
      if (err instanceof ApiError) {
        errorMessage.value = err.message
      } else {
        errorMessage.value = 'No se pudieron cargar los productos del catálogo.'
      }
    } finally {
      isLoading.value = false
    }
  }

  async function fetchMetadata() {
    isMetadataLoading.value = true
    try {
      const [cats, brs] = await Promise.all([
        catalogService.getCategories(userStore.token),
        catalogService.getBrands(userStore.token),
      ])
      categories.value = cats
      brands.value = brs
    } catch {
      // Metadatos no críticos para frenar la UI, pero logueados internamente
    } finally {
      isMetadataLoading.value = false
    }
  }

  function setSearchTerm(term: string) {
    filters.value.searchTerm = term
    filters.value.page = 1
    fetchProducts()
  }

  function setCategory(catId: number | null) {
    filters.value.categoryId = catId
    filters.value.page = 1
    fetchProducts()
  }

  function setBrand(brId: number | null) {
    filters.value.brandId = brId
    filters.value.page = 1
    fetchProducts()
  }

  function setOrderBy(orderBy: 'name_asc' | 'name_desc' | 'price_asc' | 'price_desc' | undefined) {
    filters.value.orderBy = orderBy
    filters.value.page = 1
    fetchProducts()
  }

  function setStockFilter(onlyInStock?: boolean) {
    filters.value.onlyInStock = onlyInStock
    filters.value.page = 1
    fetchProducts()
  }

  function resetFilters() {
    filters.value = {
      page: 1,
      pageSize: 12,
      searchTerm: '',
      categoryId: null,
      brandId: null,
      onlyInStock: undefined,
      orderBy: undefined,
    }
    fetchProducts(1)
  }

  async function adjustStock(productId: number, payload: StockAdjustmentPayload) {
    try {
      const updated = await catalogService.adjustStock(productId, payload, userStore.token)
      // Actualizar el producto en la lista local reactiva
      const index = products.value.findIndex((p) => p.id === productId)
      if (index !== -1) {
        products.value[index] = {
          ...products.value[index],
          stockUnits: updated.stockUnits,
          isLowStock: updated.isLowStock,
        }
      }
      return updated
    } catch (err) {
      if (err instanceof ApiError) {
        throw err
      }
      throw new Error('Error al ajustar el stock del producto.')
    }
  }

  async function uploadBulkImport(file: File) {
    isImporting.value = true
    try {
      const response = await catalogService.uploadBulkImport(file, userStore.token)
      // Iniciar polling
      await pollBulkImport(response.taskId)
      return response
    } finally {
      isImporting.value = false
    }
  }

  async function pollBulkImport(taskId: string): Promise<BulkImportStatusDto> {
    const status = await catalogService.getBulkImportStatus(taskId, userStore.token)
    bulkImportStatus.value = status

    // Si aún está en cola o procesando, esperar 1.5s y volver a consultar
    if (status.status === 1 || status.status === 2) {
      await new Promise((resolve) => setTimeout(resolve, 1500))
      return pollBulkImport(taskId)
    }

    // Si completó con éxito, refrescar productos
    if (status.status === 3) {
      await fetchProducts()
    }

    return status
  }

  return {
    products,
    totalCount,
    page,
    pageSize,
    totalPages,
    hasPreviousPage,
    hasNextPage,
    categories,
    brands,
    filters,
    isLoading,
    isMetadataLoading,
    errorMessage,
    selectedProduct,
    bulkImportStatus,
    isImporting,
    hasProducts,
    lowStockCount,
    outOfStockCount,
    fetchProducts,
    fetchMetadata,
    setSearchTerm,
    setCategory,
    setBrand,
    setOrderBy,
    setStockFilter,
    resetFilters,
    adjustStock,
    uploadBulkImport,
    pollBulkImport,
  }
})
