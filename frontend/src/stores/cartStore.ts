import { defineStore } from 'pinia'
import type { CartItem } from '@/types/ordering'
import type { ProductResponseDto } from '@/types/catalog'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [] as CartItem[],
    minimumOrderAmount: 50000.0,
    isCartOpen: false,
  }),

  getters: {
    subtotal: (state): number => {
      const sum = state.items.reduce((acc, item) => acc + item.subtotal, 0)
      return Math.round(sum * 100) / 100
    },

    vatAmount(): number {
      return Math.round(this.subtotal * 0.21 * 100) / 100
    },

    totalAmount(): number {
      return Math.round((this.subtotal + this.vatAmount) * 100) / 100
    },

    totalBoxes: (state): number => {
      return state.items.reduce((acc, item) => acc + item.boxes, 0)
    },

    totalUnits: (state): number => {
      return state.items.reduce((acc, item) => acc + item.units, 0)
    },

    itemCount: (state): number => {
      return state.items.length
    },

    isMinimumMet(): boolean {
      return this.totalAmount >= this.minimumOrderAmount
    },

    remainingForMinimum(): number {
      return Math.max(0, Math.round((this.minimumOrderAmount - this.totalAmount) * 100) / 100)
    },

    progressPercent(): number {
      if (this.totalAmount <= 0) return 0
      return Math.min(100, Math.round((this.totalAmount / this.minimumOrderAmount) * 100))
    },
  },

  actions: {
    openCart() {
      this.isCartOpen = true
    },

    closeCart() {
      this.isCartOpen = false
    },

    toggleCart() {
      this.isCartOpen = !this.isCartOpen
    },

    addOrUpdateProduct(product: ProductResponseDto, boxesToAdd = 0, unitsToAdd = 0) {
      if (boxesToAdd <= 0 && unitsToAdd <= 0) return

      const existingIndex = this.items.findIndex(i => String(i.productId) === String(product.id))
      const unitsPerBox = product.unitsPerBox > 0 ? product.unitsPerBox : 1
      const wholesalePrice = product.wholesalePrice
      const boxPrice = product.boxPrice > 0 ? product.boxPrice : wholesalePrice * unitsPerBox

      if (existingIndex >= 0) {
        const item = this.items[existingIndex]
        item.boxes += boxesToAdd
        item.units += unitsToAdd + (boxesToAdd * unitsPerBox)
        item.subtotal = Math.round(((item.boxes * boxPrice) + ((item.units - (item.boxes * unitsPerBox)) * wholesalePrice)) * 100) / 100
      } else {
        const totalUnits = unitsToAdd + (boxesToAdd * unitsPerBox)
        const subtotal = Math.round(((boxesToAdd * boxPrice) + (unitsToAdd * wholesalePrice)) * 100) / 100

        this.items.push({
          productId: product.id,
          barcode: product.barcode,
          productName: product.name,
          wholesalePrice,
          boxPrice,
          unitsPerBox,
          boxes: boxesToAdd,
          units: totalUnits,
          subtotal,
        })
      }
    },

    updateItemQuantities(productId: string | number, boxes: number, looseUnits: number) {
      const item = this.items.find(i => String(i.productId) === String(productId))
      if (!item) return

      if (boxes <= 0 && looseUnits <= 0) {
        this.removeItem(productId)
        return
      }

      item.boxes = Math.max(0, boxes)
      const validLoose = Math.max(0, looseUnits)
      item.units = validLoose + (item.boxes * item.unitsPerBox)
      item.subtotal = Math.round(((item.boxes * item.boxPrice) + (validLoose * item.wholesalePrice)) * 100) / 100
    },

    removeItem(productId: string | number) {
      this.items = this.items.filter(i => String(i.productId) !== String(productId))
    },

    clearCart() {
      this.items = []
      this.isCartOpen = false
    },
  },

  persist: true,
})
