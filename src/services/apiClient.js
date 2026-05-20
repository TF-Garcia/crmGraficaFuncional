const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'
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
    throw new Error(error.message ?? 'Falha na requisicao.')
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export const api = {
  login: (payload) => apiRequest('/api/auth/login', { method: 'POST', body: JSON.stringify(payload) }),
  register: (payload) => apiRequest('/api/auth/register', { method: 'POST', body: JSON.stringify(payload) }),
  products: () => apiRequest('/api/catalogo/produtos'),
  quote: (payload) => apiRequest('/api/orcamentos/calcular', { method: 'POST', body: JSON.stringify(payload) }),
  createOrder: (payload) => apiRequest('/api/pedidos', { method: 'POST', body: JSON.stringify(payload) }),
  myOrders: () => apiRequest('/api/pedidos/meus'),
  createPaymentPreference: (orderId) =>
    apiRequest('/api/pagamentos/mercado-pago/preferencia', {
      method: 'POST',
      body: JSON.stringify({ orderId }),
    }),
}
