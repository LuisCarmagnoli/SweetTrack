import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import CatalogFilters from '@/components/catalog/CatalogFilters.vue'
import type { CategoryDto, BrandDto } from '@/types/catalog'

describe('CatalogFilters.vue - Barra de Filtros y Búsqueda', () => {
  const mockCategories: CategoryDto[] = [
    { id: 1, name: 'Chocolates', slug: 'chocolates', description: null, isActive: true, productsCount: 10 },
    { id: 2, name: 'Alfajores', slug: 'alfajores', description: null, isActive: true, productsCount: 6 },
  ]

  const mockBrands: BrandDto[] = [
    { id: 1, name: 'Arcor', logoUrl: null, isActive: true, productsCount: 8 },
    { id: 2, name: 'Mondelez', logoUrl: null, isActive: true, productsCount: 5 },
  ]

  function createWrapper(propsOverrides = {}) {
    return mount(CatalogFilters, {
      props: {
        categories: mockCategories,
        brands: mockBrands,
        searchTerm: '',
        selectedCategory: null,
        selectedBrand: null,
        selectedOrderBy: '',
        onlyInStock: false,
        ...propsOverrides,
      },
    })
  }

  it('renderiza las opciones de categorías y marcas correctamente', () => {
    const wrapper = createWrapper()
    const options = wrapper.findAll('option')
    const optionTexts = options.map((o) => o.text())

    expect(optionTexts).toContain('Chocolates (10)')
    expect(optionTexts).toContain('Alfajores (6)')
    expect(optionTexts).toContain('Arcor')
    expect(optionTexts).toContain('Mondelez')
  })

  it('emite update:selectedCategory al cambiar la categoría seleccionada', async () => {
    const wrapper = createWrapper()
    const selects = wrapper.findAll('select')
    const categorySelect = selects[0]

    await categorySelect.setValue('2')

    expect(wrapper.emitted('update:selectedCategory')).toHaveLength(1)
    expect(wrapper.emitted('update:selectedCategory')![0]).toEqual([2])
  })

  it('emite update:selectedBrand al cambiar la marca', async () => {
    const wrapper = createWrapper()
    const selects = wrapper.findAll('select')
    const brandSelect = selects[1]

    await brandSelect.setValue('1')

    expect(wrapper.emitted('update:selectedBrand')).toHaveLength(1)
    expect(wrapper.emitted('update:selectedBrand')![0]).toEqual([1])
  })

  it('emite update:selectedOrderBy al cambiar el orden', async () => {
    const wrapper = createWrapper()
    const selects = wrapper.findAll('select')
    const orderSelect = selects[2]

    await orderSelect.setValue('price_asc')

    expect(wrapper.emitted('update:selectedOrderBy')).toHaveLength(1)
    expect(wrapper.emitted('update:selectedOrderBy')![0]).toEqual(['price_asc'])
  })

  it('emite update:onlyInStock al alternar el botón de stock', async () => {
    const wrapper = createWrapper({ onlyInStock: false })
    const stockButton = wrapper.findAll('button').find((b) => b.text().includes('Solo productos en stock'))

    expect(stockButton).toBeDefined()
    await stockButton!.trigger('click')

    expect(wrapper.emitted('update:onlyInStock')).toHaveLength(1)
    expect(wrapper.emitted('update:onlyInStock')![0]).toEqual([true])
  })

  it('emite reset al presionar Limpiar Filtros', async () => {
    const wrapper = createWrapper()
    const resetButton = wrapper.findAll('button').find((b) => b.text().includes('Limpiar Filtros'))

    expect(resetButton).toBeDefined()
    await resetButton!.trigger('click')

    expect(wrapper.emitted('reset')).toHaveLength(1)
  })
})
