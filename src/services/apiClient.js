const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5179'
const TOKEN_KEY = 'printflow_token'

export function saveApiToken(token) {
  localStorage.setItem(TOKEN_KEY, token)
}

export function clearApiToken() {
  localStorage.removeItem(TOKEN_KEY)
}

export async function apiRequest(path, options = {}) {
  const token = localStorage.getItem(TOKEN_KEY)
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Falha na requisicao.' }))
    const validationErrors = error.errors
      ? Object.values(error.errors).flat().filter(Boolean).join(' ')
      : ''
    throw new Error(error.message || error.title || validationErrors || 'Falha na requisicao.')
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export const api = {
  login: (payload) => apiRequest('/api/auth/login', { method: 'POST', body: JSON.stringify(payload) }),
  register: (payload) => apiRequest('/api/auth/register', { method: 'POST', body: JSON.stringify(payload) }),
  forgotPassword: (payload) => apiRequest('/api/auth/forgot-password', { method: 'POST', body: JSON.stringify(payload) }),
  resetPassword: (payload) => apiRequest('/api/auth/reset-password', { method: 'POST', body: JSON.stringify(payload) }),
  profile: () => apiRequest('/api/perfil'),
  updateProfile: (payload) => apiRequest('/api/perfil', { method: 'PUT', body: JSON.stringify(payload) }),
  products: () => apiRequest('/api/catalogo/produtos'),
  adminProducts: () => apiRequest('/api/admin/produtos'),
  createProduct: (payload) => apiRequest('/api/admin/produtos', { method: 'POST', body: JSON.stringify(payload) }),
  updateProduct: (productId, payload) => apiRequest(`/api/admin/produtos/${productId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deleteProduct: (productId) => apiRequest(`/api/admin/produtos/${productId}`, { method: 'DELETE' }),
  quote: (payload) => apiRequest('/api/orcamentos/calcular', { method: 'POST', body: JSON.stringify(payload) }),
  saveQuote: (payload) => apiRequest('/api/orcamentos', { method: 'POST', body: JSON.stringify(payload) }),
  updateQuote: (quoteId, payload) => apiRequest(`/api/orcamentos/${quoteId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  myQuotes: () => apiRequest('/api/orcamentos/meus'),
  convertQuote: (quoteId, payload) => apiRequest(`/api/orcamentos/${quoteId}/converter`, { method: 'POST', body: JSON.stringify(payload) }),
  createOrder: (payload) => apiRequest('/api/pedidos', { method: 'POST', body: JSON.stringify(payload) }),
  updateOrder: (orderId, payload) => apiRequest(`/api/pedidos/${orderId}`, { method: 'PUT', body: JSON.stringify(payload) }),
  cancelOrder: (orderId) => apiRequest(`/api/pedidos/${orderId}/cancelar`, { method: 'POST' }),
  refundOrder: (orderId) => apiRequest(`/api/pedidos/${orderId}/estorno`, { method: 'POST' }),
  myOrders: () => apiRequest('/api/pedidos/meus'),
  adminDashboard: () => apiRequest('/api/admin/dashboard'),
  adminOrders: () => apiRequest('/api/admin/pedidos'),
  adminCustomers: () => apiRequest('/api/admin/clientes'),
  adminInventory: () => apiRequest('/api/admin/estoque'),
  adminSettings: () => apiRequest('/api/admin/configuracoes'),
  publicSettings: () => apiRequest('/api/admin/configuracoes-publicas'),
  updateAdminSettings: (payload) => apiRequest('/api/admin/configuracoes', { method: 'PUT', body: JSON.stringify(payload) }),
  updateOrderStatus: (orderId, payload) => apiRequest(`/api/admin/pedidos/${orderId}/status`, { method: 'PUT', body: JSON.stringify(payload) }),
  confirmManualPayment: (orderId, payload) => apiRequest(`/api/pagamentos/${orderId}/confirmar-manual`, { method: 'POST', body: JSON.stringify(payload) }),
  mercadoPagoConfig: () => apiRequest('/api/pagamentos/mercado-pago/config'),
  createPixPayment: (orderId) => apiRequest(`/api/pagamentos/${orderId}/pix`, { method: 'POST' }),
  payWithCard: (orderId, payload) => apiRequest(`/api/pagamentos/${orderId}/cartao`, { method: 'POST', body: JSON.stringify(payload) }),
  stockMovement: (payload) => apiRequest('/api/admin/estoque/movimentacoes', { method: 'POST', body: JSON.stringify(payload) }),
}
