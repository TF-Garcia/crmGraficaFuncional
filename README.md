# crmGraficaFuncional

Plataforma demonstrativa completa para grafica, armazem e empresas com catalogo, orcamento, pedidos, estoque, producao, pagamentos e CRM administrativo.

## Como rodar

```bash
npm install
npm run dev
```

Depois acesse `http://localhost:5173`.

Para validar build de producao:

```bash
npm run build
```

## O que esta pronto

- Landing page publica responsiva com CTA de orcamento, WhatsApp, catalogo e portfolio.
- Catalogo com categorias, imagens, detalhes, preco base, prazo base e botao de orcamento.
- Orcamento automatico com regras simuladas para quantidade, tamanho, material, impressao, acabamento, urgencia e entrega.
- Login/cadastro simulados com entrada como cliente ou administrador.
- Area do cliente com dashboard, pedidos, orcamentos, perfil, reenvio de arte, aprovacao e atendimento.
- Painel admin com sidebar, dashboard, pedidos, clientes, produtos, estoque, producao Kanban, pagamentos, entregas, relatorios e configuracoes.
- Camada de pagamento simulada para Pix, cartao e pagamento na retirada, preparada para Mercado Pago ou Asaas.

## Pontos simulados

- Autenticacao e permissoes usam `localStorage`.
- Dados vem de `src/data/mockData.js`.
- Orcamentos sao calculados em `src/services/quoteService.js`.
- Pagamentos sao simulados em `src/services/paymentService.js`.
- Upload de arte captura o nome do arquivo, mas ainda nao envia para storage/API.

## Backend real adicionado

Foi criada a API em `API/PrintFlowApi`, seguindo a organizacao do BarberStyle:

- ASP.NET Core com controllers REST.
- Entity Framework com MySQL real via Pomelo.
- Migration inicial em `API/PrintFlowApi/Migrations`.
- Seed inicial de admin, cliente, catalogo e estoque.
- JWT para clientes/admin/producao/atendimento/financeiro.
- Endpoints para catalogo, calculo de orcamento, pedidos, admin, estoque e Mercado Pago.
- Checkout real do Mercado Pago preparado para ambiente sandbox/producao.

Veja `API/README.md` para criar o banco MySQL local, aplicar migrations e configurar VPS/Hostinger.

## Integracoes futuras

- Ligar as telas do React aos endpoints REST criados na API.
- Enviar arquivos para storage privado e registrar historico de aprovacao de arte por pedido.
- Finalizar webhook em dominio publico HTTPS e homologar credenciais reais do Mercado Pago.
