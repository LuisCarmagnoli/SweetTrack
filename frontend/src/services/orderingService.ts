import { apiClient } from '@/services/apiClient'
import { useUserStore } from '@/stores/userStore'
import type {
  CreateOrderRequestDto,
  DispatchBoardDto,
  OrderDetailDto,
  OrderResponseDto,
  StatusTransitionRequestDto,
  WsTicketResponseDto,
} from '@/types/ordering'

export const orderingService = {
  async createOrder(request: CreateOrderRequestDto): Promise<OrderResponseDto> {
    const userStore = useUserStore()
    return await apiClient<OrderResponseDto>('/api/orders', {
      method: 'POST',
      token: userStore.token,
      body: JSON.stringify(request),
    })
  },

  async getMyOrders(page = 1, pageSize = 20): Promise<OrderResponseDto[]> {
    const userStore = useUserStore()
    return await apiClient<OrderResponseDto[]>(`/api/orders/my-orders?page=${page}&pageSize=${pageSize}`, {
      method: 'GET',
      token: userStore.token,
    })
  },

  async getOrderById(id: string): Promise<OrderDetailDto> {
    const userStore = useUserStore()
    return await apiClient<OrderDetailDto>(`/api/orders/${id}`, {
      method: 'GET',
      token: userStore.token,
    })
  },

  async transitionStatus(id: string, request: StatusTransitionRequestDto): Promise<OrderDetailDto> {
    const userStore = useUserStore()
    return await apiClient<OrderDetailDto>(`/api/orders/${id}/status`, {
      method: 'PATCH',
      token: userStore.token,
      body: JSON.stringify(request),
    })
  },

  async getDispatchBoard(): Promise<DispatchBoardDto> {
    const userStore = useUserStore()
    return await apiClient<DispatchBoardDto>('/api/dispatch/board', {
      method: 'GET',
      token: userStore.token,
    })
  },

  async getWsTicket(): Promise<WsTicketResponseDto> {
    const userStore = useUserStore()
    return await apiClient<WsTicketResponseDto>('/api/orders/ws-ticket', {
      method: 'POST',
      token: userStore.token,
    })
  },

  async downloadWaybillPdf(id: string, orderNumber = 'remito'): Promise<void> {
    const userStore = useUserStore()
    const response = await fetch(`/api/dispatch/orders/${id}/remito-pdf`, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${userStore.token}`,
      },
      signal: AbortSignal.timeout(30000),
    })

    if (!response.ok) {
      throw new Error('No se pudo generar o descargar el remito en PDF.')
    }

    const blob = await response.blob()
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `Remito-${orderNumber}.pdf`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  },
}

