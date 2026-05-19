export const paymentMethods = [
  { id: 'pix', label: 'Pix', requiresOnlineConfirmation: true },
  { id: 'card', label: 'Cartão', requiresOnlineConfirmation: true },
  { id: 'pickup', label: 'Pagar na retirada', requiresOnlineConfirmation: false },
]

export function createPaymentIntent(order, method) {
  const reference = `PF-${order.id}-${Date.now().toString().slice(-5)}`

  if (method === 'pickup') {
    return {
      provider: 'manual',
      reference,
      status: 'pending_pickup',
      checkoutUrl: null,
      message: 'Pagamento pendente para o dia da retirada.',
    }
  }

  return {
    provider: method === 'pix' ? 'pix-simulado' : 'cartao-simulado',
    reference,
    status: 'waiting_confirmation',
    checkoutUrl: null,
    message: 'Checkout simulado. Camada pronta para Mercado Pago ou Asaas.',
  }
}
