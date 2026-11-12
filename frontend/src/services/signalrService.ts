import * as signalR from '@microsoft/signalr'
import { orderingService } from '@/services/orderingService'
import type { OrderResponseDto } from '@/types/ordering'

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private isConnecting = false

  public async start(): Promise<void> {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return
    }

    if (this.isConnecting) {
      return
    }

    this.isConnecting = true

    try {
      // 1. Obtener ticket efímero de corta duración (30s)
      const { ticket } = await orderingService.getWsTicket()

      // 2. Construir conexión con ticket en query string sin exponer JWT
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`/hubs/orders?ticket=${ticket}`)
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .configureLogging(signalR.LogLevel.Warning)
        .build()

      await this.connection.start()
      console.log('[SignalR] Conexión establecida con OrdersHub.')
    } catch (error) {
      console.warn('[SignalR] No se pudo establecer la conexión en tiempo real:', error)
    } finally {
      this.isConnecting = false
    }
  }

  public async stop(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop()
      } catch (err) {
        console.warn('[SignalR] Error al cerrar conexión:', err)
      } finally {
        this.connection = null
      }
    }
  }

  public onOrderCreated(callback: (order: OrderResponseDto) => void): void {
    this.connection?.on('OrderCreated', callback)
  }

  public onOrderStatusChanged(callback: (payload: { orderId: string; newStatus: number; orderNumber: string }) => void): void {
    this.connection?.on('OrderStatusChanged', callback)
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }
}

export const signalrService = new SignalRService()

