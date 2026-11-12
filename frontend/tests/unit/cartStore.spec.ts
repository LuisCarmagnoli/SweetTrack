import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCartStore } from '@/stores/cartStore'
import type { ProductResponseDto } from '@/types/catalog'

describe('useCartStore - Carrito Mayorista B2B y Reglas de Negocio', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  const mockProduct: ProductResponseDto = {
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
    stockUnits: 100,
    safetyStock: 10,
    isLowStock: false,
    imageUrl: null,
    isActive: true,
  }

  it('se inicializa vacío con umbral mínimo de $50.000 ARS', () => {
    const store = useCartStore()
    expect(store.items).toHaveLength(0)
    expect(store.minimumOrderAmount).toBe(50000)
    expect(store.isCartOpen).toBe(false)
    expect(store.subtotal).toBe(0)
    expect(store.vatAmount).toBe(0)
    expect(store.totalAmount).toBe(0)
    expect(store.isMinimumMet).toBe(false)
    expect(store.remainingForMinimum).toBe(50000)
    expect(store.progressPercent).toBe(0)
  })

  it('permite abrir, cerrar y alternar el drawer del carrito', () => {
    const store = useCartStore()
    store.openCart()
    expect(store.isCartOpen).toBe(true)
    store.closeCart()
    expect(store.isCartOpen).toBe(false)
    store.toggleCart()
    expect(store.isCartOpen).toBe(true)
  })

  it('agrega un bulto y calcula subtotal, IVA (21%) y progreso', () => {
    const store = useCartStore()
    store.addOrUpdateProduct(mockProduct, 1, 0)

    expect(store.items).toHaveLength(1)
    const item = store.items[0]
    expect(item.boxes).toBe(1)
    expect(item.units).toBe(12)
    expect(item.subtotal).toBe(38400)

    // Subtotal: 38.400, IVA 21%: 8.064, Total: 46.464
    expect(store.subtotal).toBe(38400)
    expect(store.vatAmount).toBe(8064)
    expect(store.totalAmount).toBe(46464)

    // Umbral mínimo no alcanzado (faltan 3.536)
    expect(store.isMinimumMet).toBe(false)
    expect(store.remainingForMinimum).toBe(3536)
    expect(store.progressPercent).toBe(93)
  })

  it('supera el monto mínimo al agregar un segundo bulto', () => {
    const store = useCartStore()
    store.addOrUpdateProduct(mockProduct, 1, 0)
    store.addOrUpdateProduct(mockProduct, 1, 0)

    expect(store.items[0].boxes).toBe(2)
    expect(store.items[0].units).toBe(24)
    expect(store.subtotal).toBe(76800)
    expect(store.vatAmount).toBe(16128)
    expect(store.totalAmount).toBe(92928)

    // Umbral mínimo superado
    expect(store.isMinimumMet).toBe(true)
    expect(store.remainingForMinimum).toBe(0)
    expect(store.progressPercent).toBe(100)
  })

  it('actualiza cantidades por bulto y unidades sueltas con recálculo exacto', () => {
    const store = useCartStore()
    store.addOrUpdateProduct(mockProduct, 1, 0)

    // Actualiza a 1 bulto + 2 unidades sueltas: 38400 + (2 * 3200) = 44800
    store.updateItemQuantities(mockProduct.id, 1, 2)

    expect(store.items[0].boxes).toBe(1)
    expect(store.items[0].units).toBe(14)
    expect(store.items[0].subtotal).toBe(44800)
  })

  it('elimina el producto si se actualiza a 0 bultos y 0 unidades', () => {
    const store = useCartStore()
    store.addOrUpdateProduct(mockProduct, 1, 0)
    expect(store.items).toHaveLength(1)

    store.updateItemQuantities(mockProduct.id, 0, 0)
    expect(store.items).toHaveLength(0)
  })

  it('elimina un ítem específico con removeItem', () => {
    const store = useCartStore()
    store.addOrUpdateProduct(mockProduct, 2, 0)
    expect(store.items).toHaveLength(1)

    store.removeItem(mockProduct.id)
    expect(store.items).toHaveLength(0)
  })

  it('vacía el carrito por completo con clearCart', () => {
    const store = useCartStore()
    store.addOrUpdateProduct(mockProduct, 2, 0)
    store.openCart()

    store.clearCart()
    expect(store.items).toHaveLength(0)
    expect(store.isCartOpen).toBe(false)
  })
})
