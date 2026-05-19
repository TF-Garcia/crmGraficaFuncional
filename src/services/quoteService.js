const urgencyMultiplier = {
  normal: { price: 1, days: 0 },
  expressa: { price: 1.25, days: -1 },
  urgente: { price: 1.45, days: -2 },
}

const deliveryFees = {
  retirada: 0,
  entrega: 28,
}

export function calculateQuote(product, config) {
  if (!product) {
    return {
      subtotal: 0,
      urgencyFee: 0,
      deliveryFee: 0,
      total: 0,
      estimatedDays: 0,
      details: [],
    }
  }

  const quantity = Number(config.quantity || product.quantities[0] || 1)
  const quantityFactor = Math.max(quantity / 100, 1)
  const size = product.sizes.find((item) => item.name === config.size) ?? product.sizes[0]
  const material = product.materials.find((item) => item.name === config.material) ?? product.materials[0]
  const printMode = product.printModes.find((item) => item.name === config.printMode) ?? product.printModes[0]
  const finishing = product.finishings.find((item) => item.name === config.finishing) ?? product.finishings[0]
  const urgency = urgencyMultiplier[config.urgency] ?? urgencyMultiplier.normal
  const deliveryFee = deliveryFees[config.delivery] ?? 0

  const base = product.basePrice * quantityFactor
  const variable = size.price + material.price + printMode.price + finishing.price
  const subtotal = Math.round((base + variable * quantityFactor) * 100) / 100
  const urgencyFee = Math.round((subtotal * (urgency.price - 1)) * 100) / 100
  const total = Math.round((subtotal + urgencyFee + deliveryFee) * 100) / 100
  const estimatedDays = Math.max(1, product.baseDeadline + size.days + material.days + finishing.days + urgency.days)

  return {
    subtotal,
    urgencyFee,
    deliveryFee,
    total,
    estimatedDays,
    details: [
      `${quantity.toLocaleString('pt-BR')} unidades`,
      size.name,
      material.name,
      printMode.name,
      finishing.name,
      config.delivery === 'entrega' ? 'Entrega local' : 'Retirada no balcão',
    ],
  }
}
