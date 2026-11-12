import { defineStore } from 'pinia'
import { orderingService } from '@/services/orderingService'
import { signalrService } from '@/services/signalrService'
import type {
  CreateOrderRequestDto,
  DispatchBoardDto,
  OrderDetailDto,
  OrderResponseDto,
  OrderStatus,
} from '@/types/ordering'

export const useOrderStore = defineStore('order', {
  state: () => ({
    myOrders: [] as OrderResponseDto[],
    currentOrder: null as OrderDetailDto | null,
    dispatchBoard: null as DispatchBoardDto | null,
    isLoading: false,
    error: null as string | null,
    isSignalRConnected: false,
  }),

  actions: {
    async fetchMyOrders(page = 1, pageSize = 20) {
      this.isLoading = true
      this.error = null
      try {
        this.myOrders = await orderingService.getMyOrders(page, pageSize)
      } catch (err: any) {
        this.error = err.message || 'Error al cargar historial de pedidos.'
      } finally {
        this.isLoading = false
      }
    },

    async fetchOrderById(id: string) {
      this.isLoading = true
      this.error = null
      try {
        this.currentOrder = await orderingService.getOrderById(id)
      } catch (err: any) {
        this.error = err.message || 'Error al cargar detalle del pedido.'
      } finally {
        this.isLoading = false
      }
    },

    async fetchDispatchBoard() {
      this.isLoading = true
      this.error = null
      try {
        this.dispatchBoard = await orderingService.getDispatchBoard()
      } catch (err: any) {
        this.error = err.message || 'Error al cargar tablero de despacho.'
      } finally {
        this.isLoading = false
      }
    },

    async createOrder(request: CreateOrderRequestDto): Promise<OrderResponseDto> {
      this.isLoading = true
      this.error = null
      try {
        const order = await orderingService.createOrder(request)
        this.myOrders.unshift(order)
        return order
      } catch (err: any) {
        this.error = err.message || 'No se pudo crear el pedido.'
        throw err
      } finally {
        this.isLoading = false
      }
    },

    async transitionOrderStatus(id: string, targetStatus: OrderStatus, notes?: string | null) {
      this.isLoading = true
      this.error = null
      try {
        const updated = await orderingService.transitionStatus(id, { targetStatus, notes })
        this.currentOrder = updated

        // Actualizar en lista local
        const idx = this.myOrders.findIndex(o => o.id === id)
        if (idx >= 0) {
          this.myOrders[idx].status = updated.status
        }

        // Si tenemos tablero de despacho abierto, refrescarlo
        if (this.dispatchBoard) {
          await this.fetchDispatchBoard()
        }

        return updated
      } catch (err: any) {
        this.error = err.message || 'Error al actualizar estado del pedido.'
        throw err
      } finally {
        this.isLoading = false
      }
    },

    async downloadWaybillPdf(id: string, orderNumber: string) {
      try {
        await orderingService.downloadWaybillPdf(id, orderNumber)
      } catch (err: any) {
        this.error = err.message || 'Error al descargar el remito comercial.'
        throw err
      }
    },

    async initRealtime() {
      if (this.isSignalRConnected) return

      await signalrService.start()
      this.isSignalRConnected = signalrService.isConnected()

      // Evento: Cambio de estado de un pedido
      signalrService.onOrderStatusChanged((payload) => {
        // 1. Actualizar en pedido activo si coincide
        if (this.currentOrder && this.currentOrder.id === payload.orderId) {
          this.currentOrder.status = payload.newStatus as OrderStatus
        }

        // 2. Actualizar en listado de pedidos
        const order = this.myOrders.find(o => o.id === payload.orderId)
        if (order) {
          order.status = payload.newStatus as OrderStatus
        }

        // 3. Si hay tablero Kanban, refrescar para reflejar columnas
        if (this.dispatchBoard) {
          this.fetchDispatchBoard()
        }
      })

      // Evento: Nuevo pedido entrante (para operadores de depósito)
      signalrService.onOrderCreated((order) => {
        if (this.dispatchBoard) {
          this.dispatchBoard.enPreparacion.unshift(order)
        }
      })
    },

    async teardownRealtime() {
      await signalrService.stop()
      this.isSignalRConnected = false
    },
  },
})
