import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import ProductCard from '@/components/catalog/ProductCard.vue'
import { useCartStore } from '@/stores/cartStore'
import type { ProductResponseDto } from '@/types/catalog'

describe('ProductCard.vue - Tarjeta de Producto B2B', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  const baseProduct: ProductResponseDto = {
    id: 10,
    barcode: '7791234567890',
    name: 'Chocolate Cofler Block 300g',
    brandId: 1,
    brandName: 'Arcor',
    categoryId: 1,
    categoryName: 'Chocolates',
    wholesalePrice: 3200,
    boxPrice: 38400,
    unitsPerBox: 12,
    stockUnits: 45,
    safetyStock: 10,
    isLowStock: false,
    imageUrl: null,
    isActive: true,
  }

  function createWrapper(propsOverrides = {}) {
    return mount(ProductCard, {
      props: {
        product: { ...baseProduct },
        isAdmin: false,
        ...propsOverrides,
      },
    })
  }

  it('renderiza el nombre del producto, marca y código de barras', () => {
    const wrapper = createWrapper()
    expect(wrapper.text()).toContain('Chocolate Cofler Block 300g')
    expect(wrapper.text()).toContain('Arcor')
    expect(wrapper.text()).toContain('7791234567890')
    expect(wrapper.text()).toContain('Chocolates')
  })

  it('muestra badge "En Stock" cuando el stock es suficiente', () => {
    const wrapper = createWrapper()
    expect(wrapper.text()).toContain('45 en stock')
  })

  it('muestra badge "Bajo Stock" cuando isLowStock es true', () => {
    const wrapper = createWrapper({
      product: {
        ...baseProduct,
        stockUnits: 5,
        isLowStock: true,
      },
    })
    expect(wrapper.text()).toContain('Bajo Stock (5)')
  })

  it('muestra badge "Sin Stock" cuando stockUnits es 0', () => {
    const wrapper = createWrapper({
      product: {
        ...baseProduct,
        stockUnits: 0,
        isLowStock: true,
      },
    })
    expect(wrapper.text()).toContain('Sin Stock')
  })

  it('no muestra el botón de Ajustar Stock si el usuario no es Admin', () => {
    const wrapper = createWrapper({ isAdmin: false })
    expect(wrapper.text()).not.toContain('Ajustar Stock')
    expect(wrapper.text()).toContain('+1 Bulto al Carrito')
  })

  it('agrega un bulto al carrito cuando el cliente hace clic en +1 Bulto al Carrito', async () => {
    const wrapper = createWrapper({ isAdmin: false })
    const cartStore = useCartStore()

    const addCartButton = wrapper.findAll('button').find((btn) => btn.text().includes('+1 Bulto al Carrito'))
    expect(addCartButton).toBeDefined()
    await addCartButton!.trigger('click')

    expect(cartStore.items).toHaveLength(1)
    expect(cartStore.items[0].boxes).toBe(1)
    expect(cartStore.isCartOpen).toBe(true)
  })

  it('muestra el botón de Ajustar Stock y emite evento al hacer clic si es Admin', async () => {
    const wrapper = createWrapper({ isAdmin: true })
    expect(wrapper.text()).toContain('Ajustar Stock')

    const adjustButton = wrapper.findAll('button').find((btn) => btn.text().includes('Ajustar Stock'))
    expect(adjustButton).toBeDefined()
    await adjustButton!.trigger('click')

    expect(wrapper.emitted('adjust-stock')).toHaveLength(1)
    expect(wrapper.emitted('adjust-stock')![0]).toEqual([baseProduct])
  })
})
