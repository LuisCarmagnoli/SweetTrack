import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/userStore'
import LoginView from '@/views/auth/LoginView.vue'
import DashboardView from '@/views/dashboard/DashboardView.vue'
import CatalogView from '@/views/catalog/CatalogView.vue'
import MyOrdersView from '@/views/orders/MyOrdersView.vue'
import DispatchBoardView from '@/views/dispatch/DispatchBoardView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/dashboard',
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: {
        guestOnly: true,
        title: 'Iniciar Sesión | SweetTrack B2B',
      },
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: DashboardView,
      meta: {
        requiresAuth: true,
        title: 'Dashboard | SweetTrack B2B',
      },
    },
    {
      path: '/catalog',
      name: 'catalog',
      component: CatalogView,
      meta: {
        requiresAuth: true,
        title: 'Catálogo de Golosinas | SweetTrack B2B',
      },
    },
    {
      path: '/my-orders',
      name: 'my-orders',
      component: MyOrdersView,
      meta: {
        requiresAuth: true,
        title: 'Mis Pedidos | SweetTrack B2B',
      },
    },
    {
      path: '/dispatch',
      name: 'dispatch',
      component: DispatchBoardView,
      meta: {
        requiresAuth: true,
        adminOnly: true,
        title: 'Tablero de Despacho | SweetTrack B2B',
      },
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/login',
    },
  ],
})

// Navigation Guard de Autenticación y Roles
router.beforeEach((to, _from, next) => {
  const userStore = useUserStore()

  // Actualizar título de la página
  if (to.meta.title) {
    document.title = String(to.meta.title)
  }

  // Si requiere autenticación y no hay sesión activa
  if (to.meta.requiresAuth && !userStore.isAuthenticated) {
    next({
      path: '/login',
      query: { redirect: to.fullPath },
    })
    return
  }

  // Si es ruta exclusiva para administradores y no tiene el rol
  if (to.meta.adminOnly && !userStore.isAdmin) {
    next({ path: '/dashboard' })
    return
  }

  // Si es ruta exclusiva para no-autenticados (ej: login) y ya está logueado
  if (to.meta.guestOnly && userStore.isAuthenticated) {
    next({ path: '/dashboard' })
    return
  }

  next()
})

export default router
