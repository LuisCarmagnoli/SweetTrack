export const OrderStatus = {
  PendienteConfirmacion: 1,
  EnPreparacion: 2,
  ListoParaDespacho: 3,
  EnReparto: 4,
  Entregado: 5,
  NoEntregado: 6,
  Cancelado: 7,
} as const

export type OrderStatus = typeof OrderStatus[keyof typeof OrderStatus]

export const OrderStatusLabels: Record<OrderStatus, string> = {
  [OrderStatus.PendienteConfirmacion]: 'Pendiente de Confirmación',
  [OrderStatus.EnPreparacion]: 'En Preparación',
  [OrderStatus.ListoParaDespacho]: 'Listo para Despacho',
  [OrderStatus.EnReparto]: 'En Reparto',
  [OrderStatus.Entregado]: 'Entregado',
  [OrderStatus.NoEntregado]: 'No Entregado',
  [OrderStatus.Cancelado]: 'Cancelado',
}

export const PaymentMethod = {
  CuentaCorriente: 1,
  TransferenciaBancaria: 2,
  EfectivoContraEntrega: 3,
} as const

export type PaymentMethod = typeof PaymentMethod[keyof typeof PaymentMethod]

export const PaymentMethodLabels: Record<PaymentMethod, string> = {
  [PaymentMethod.CuentaCorriente]: 'Cuenta Corriente',
  [PaymentMethod.TransferenciaBancaria]: 'Transferencia Bancaria',
  [PaymentMethod.EfectivoContraEntrega]: 'Efectivo contra Entrega',
}

export interface CreateOrderItemDto {
  productId: string
  barcode: string
  productName: string
  units: number
  boxes: number
  unitPrice: number
}

export interface CreateOrderRequestDto {
  razonSocialKiosco: string
  cuitKiosco: string
  direccionEntrega: string
  paymentMethod: PaymentMethod
  items: CreateOrderItemDto[]
  deliveryNotes?: string | null
}

export interface OrderItemResponseDto {
  id: string
  productId: string
  barcode: string
  productName: string
  units: number
  boxes: number
  unitPrice: number
  lineTotal: number
}

export interface StatusHistoryDto {
  id: string
  previousStatus: OrderStatus
  newStatus: OrderStatus
  modifiedByUserId: string
  notes?: string | null
  timestampUtc: string
}

export interface OrderResponseDto {
  id: string
  orderNumber: string
  kioskBusinessName: string
  subtotal: number
  vatAmount: number
  totalAmount: number
  status: OrderStatus
  paymentMethod: PaymentMethod
  totalItems: number
  createdAtUtc: string
}

export interface OrderDetailDto {
  id: string
  orderNumber: string
  kioskUserId: string
  kioskBusinessName: string
  kioskCuit: string
  deliveryAddress: string
  subtotal: number
  vatAmount: number
  totalAmount: number
  status: OrderStatus
  paymentMethod: PaymentMethod
  deliveryNotes?: string | null
  createdAtUtc: string
  updatedAtUtc?: string | null
  items: OrderItemResponseDto[]
  statusHistory: StatusHistoryDto[]
  availableTransitions: OrderStatus[]
}

export interface StatusTransitionRequestDto {
  targetStatus: OrderStatus
  notes?: string | null
}

export interface DispatchBoardDto {
  enPreparacion: OrderResponseDto[]
  listoParaDespacho: OrderResponseDto[]
  enReparto: OrderResponseDto[]
}

export interface WsTicketResponseDto {
  ticket: string
  expiresAtUtc: string
}

export interface CartItem {
  productId: string | number
  barcode: string
  productName: string
  wholesalePrice: number
  boxPrice: number
  unitsPerBox: number
  boxes: number
  units: number
  subtotal: number
}
