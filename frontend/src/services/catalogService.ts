import { apiClient } from '@/services/apiClient'
import type {
  ProductResponseDto,
  ProductDetailDto,
  BrandDto,
  CategoryDto,
  PagedResultDto,
  CatalogFilterParams,
  StockAdjustmentPayload,
  BulkImportResponseDto,
  BulkImportStatusDto,
} from '@/types/catalog'

export const catalogService = {
  async getProducts(
    params: CatalogFilterParams = {},
    token?: string | null
  ): Promise<PagedResultDto<ProductResponseDto>> {
    const query = new URLSearchParams()
    if (params.page) query.append('page', params.page.toString())
    if (params.pageSize) query.append('pageSize', params.pageSize.toString())
    if (params.searchTerm?.trim()) query.append('searchTerm', params.searchTerm.trim())
    if (params.categoryId) query.append('categoryId', params.categoryId.toString())
    if (params.brandId) query.append('brandId', params.brandId.toString())
    if (params.minPrice != null) query.append('minPrice', params.minPrice.toString())
    if (params.maxPrice != null) query.append('maxPrice', params.maxPrice.toString())
    if (params.onlyInStock != null) query.append('onlyInStock', params.onlyInStock.toString())
    if (params.onlyActive != null) query.append('onlyActive', params.onlyActive.toString())
    if (params.orderBy) query.append('orderBy', params.orderBy)

    const url = `/api/catalog/products${query.toString() ? `?${query.toString()}` : ''}`
    return apiClient<PagedResultDto<ProductResponseDto>>(url, { token })
  },

  async getProductById(id: number, token?: string | null): Promise<ProductDetailDto> {
    return apiClient<ProductDetailDto>(`/api/catalog/products/${id}`, { token })
  },

  async getCategories(token?: string | null): Promise<CategoryDto[]> {
    return apiClient<CategoryDto[]>('/api/catalog/categories', { token })
  },

  async getBrands(token?: string | null): Promise<BrandDto[]> {
    return apiClient<BrandDto[]>('/api/catalog/brands', { token })
  },

  async adjustStock(
    productId: number,
    payload: StockAdjustmentPayload,
    token?: string | null
  ): Promise<ProductDetailDto> {
    return apiClient<ProductDetailDto>(`/api/catalog/products/${productId}/adjust-stock`, {
      method: 'POST',
      body: JSON.stringify(payload),
      token,
    })
  },

  async uploadBulkImport(file: File, token?: string | null): Promise<BulkImportResponseDto> {
    const formData = new FormData()
    formData.append('file', file)

    return apiClient<BulkImportResponseDto>('/api/catalog/bulk-import/excel', {
      method: 'POST',
      body: formData,
      token,
    })
  },

  async getBulkImportStatus(taskId: string, token?: string | null): Promise<BulkImportStatusDto> {
    return apiClient<BulkImportStatusDto>(`/api/catalog/bulk-import/status/${taskId}`, { token })
  },
}
