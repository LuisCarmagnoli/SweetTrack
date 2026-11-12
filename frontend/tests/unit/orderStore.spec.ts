import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useOrderStore } from '@/stores/orderStore'
import { orderingService } from '@/services/orderingService'
import { signalrService } from '@/services/signalrService'
import { OrderStatus, PaymentMethod } from '@/types/ordering'

vi.mock('@/services/orderingService', () => ({
  orderingService: {
    getMyOrders: vi.fn(),
    getOrderById: vi.fn(),
    getDispatchBoard: vi.fn(),
    createOrder: vi.fn(),
    transitionStatus: vi.fn(),
    downloadWaybillPdf: vi.fn(),
  },
}))

vi.mock('@/services/signalrService', () => {
  let statusCallback: ((p: any) => void) | null = null
  let createdCallback: ((o: any) => void) | null = null

  return {
    signalrService: {
      start: vi.fn().mockResolvedValue(undefined),
      stop: vi.fn().mockResolvedValue(undefined),
      isConnected: vi.fn().mockReturnValue(true),
      onOrderStatusChanged: vi.fn((cb) => {
        statusCallback = cb
      }),
      onOrderCreated: vi.fn((cb) => {
        createdCallback = cb
      }),
      // Métodos helper para simular eventos en pruebas
      __triggerStatusChanged: (payload: any) => statusCallback && statusCallback(payload),
      __triggerOrderCreated: (order: any) => createdCallback && createdCallback(order),
    },
  }
})

describe('useOrderStore - Flujo de Órdenes y Tiempo Real SignalR', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  const mockOrder = {
    id: 'e3b0c442-98fc-1c14-9afbf4c8996fb924',
    orderNumber: 'ORD-20261112-0001',
    kioskBusinessName: 'Kiosco Central',
    cuitKiosco: '20-12345678-9',
    deliveryAddress: 'Av. Corrientes 1000',
    status: OrderStatus.PendienteConfirmacion,
    paymentMethod: PaymentMethod.TransferenciaBancaria,
    totalItems: 12,
    subtotal: 50000,
    vatAmount: 10500,
    totalAmount: 60500,
    deliveryRouteName: null,
    createdAtUtc: '2026-11-12T12:00:00Z',
  }

  it('carga la lista de pedidos con fetchMyOrders', async () => {
    vi.mocked(orderingService.getMyOrders).mockResolvedValue([mockOrder])

    const store = useOrderStore()
    await store.fetchMyOrders()

    expect(store.myOrders).toHaveLength(1)
    expect(store.myOrders[0].orderNumber).toBe('ORD-20261112-0001')
    expect(store.isLoading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('obtiene el detalle completo de un pedido con fetchOrderById', async () => {
    const mockDetail = {
      ...mockOrder,
      deliveryNotes: 'Dejar en recepción',
      items: [],
      statusHistories: [],
    }
    vi.mocked(orderingService.getOrderById).mockResolvedValue(mockDetail)

    const store = useOrderStore()
    await store.fetchOrderById(mockOrder.id)

    expect(store.currentOrder).toBeDefined()
    expect(store.currentOrder?.orderNumber).toBe('ORD-20261112-0001')
    expect(store.isLoading).toBe(false)
  })

  it('crea una nueva orden y la añade al inicio de myOrders', async () => {
    vi.mocked(orderingService.createOrder).mockResolvedValue(mockOrder)

    const store = useOrderStore()
    const result = await store.createOrder({
      kioskBusinessName: 'Kiosco Central',
      cuitKiosco: '20-12345678-9',
      deliveryAddress: 'Av. Corrientes 1000',
      paymentMethod: PaymentMethod.TransferenciaBancaria,
      items: [{ productId: 'p1', barcode: '123', productName: 'Item', units: 1, boxes: 1, unitPrice: 50000 }],
    })

    expect(result.id).toBe(mockOrder.id)
    expect(store.myOrders).toHaveLength(1)
    expect(store.myOrders[0].id).toBe(mockOrder.id)
  })

  it('actualiza el estado de la orden con transitionOrderStatus', async () => {
    const updatedOrder = {
      ...mockOrder,
      status: OrderStatus.EnPreparacion,
      deliveryNotes: null,
      items: [],
      statusHistories: [],
    }
    vi.mocked(orderingService.transitionStatus).mockResolvedValue(updatedOrder)

    const store = useOrderStore()
    store.myOrders = [{ ...mockOrder }]

    const result = await store.transitionOrderStatus(mockOrder.id, OrderStatus.EnPreparacion, 'Iniciando')

    expect(result.status).toBe(OrderStatus.EnPreparacion)
    expect(store.myOrders[0].status).toBe(OrderStatus.EnPreparacion)
    expect(store.currentOrder?.status).toBe(OrderStatus.EnPreparacion)
  })

  it('conecta y reacciona a eventos en tiempo real de SignalR', async () => {
    const store = useOrderStore()
    store.myOrders = [{ ...mockOrder }]
    store.currentOrder = {
      ...mockOrder,
      deliveryNotes: null,
      items: [],
      statusHistories: [],
    }

    await store.initRealtime()
    expect(store.isSignalRConnected).toBe(true)

    // Simular recepción de evento SignalR de cambio de estado
    ;(signalrService as any).__triggerStatusChanged({
      orderId: mockOrder.id,
      newStatus: OrderStatus.ListoParaDespacho,
      comment: 'Empacado',
      changedAtUtc: '2026-11-12T12:30:00Z',
    })

    expect(store.currentOrder?.status).toBe(OrderStatus.ListoParaDespacho)
    expect(store.myOrders[0].status).toBe(OrderStatus.ListoParaDespacho)

    await store.teardownRealtime()
    expect(store.isSignalRConnected).toBe(false)
  })
})

