# crmGraficaFuncional

Plataforma real para grafica com catalogo, orcamento, pedidos, estoque, producao, pagamentos preparados, recuperacao de senha e CRM administrativo.

## Como rodar

API:

```bash
cd API/PrintFlowApi
dotnet run
```

Front:

```bash
npm install
npm run dev
```

Depois acesse `http://localhost:5173`.

No front, configure `VITE_API_BASE_URL` quando a API nao estiver no padrao `http://localhost:5179`.

Para validar builds:

```bash
dotnet build API/PrintFlowApi
npm run build
```

## O que esta pronto

- Landing page publica conectada ao catalogo real.
- Cadastro/login reais com JWT.
- Recuperacao de senha por email SMTP.
- Catalogo e orcamento calculados pela API.
- Criacao de orcamentos salvos e pedidos reais no MySQL.
- Area do cliente com pedidos, orcamentos e perfil editavel.
- Painel admin com dashboard, pedidos, clientes, produtos, estoque, producao, pagamentos e configuracoes.
- Pagamento Pix e cartao integrados ao Mercado Pago; pagamento no balcao confirmado direto.

## Pontos pendentes

- Upload de arte ainda registra apenas o nome do arquivo; storage privado fica para a proxima etapa.
- Para Mercado Pago em producao, configurar `MercadoPago__PublicKey`, `MercadoPago__AccessToken` e webhook publico na API.

## Backend

Foi criada a API em `API/PrintFlowApi`, seguindo a organizacao do BarberStyle:

- ASP.NET Core com controllers REST.
- Entity Framework com MySQL real via Pomelo.
- Migrations em `API/PrintFlowApi/Migrations`.
- Seed inicial de admin, cliente, catalogo, estoque e configuracoes.
- JWT para cliente/admin e estrutura para producao/atendimento/financeiro.
- Endpoints para catalogo, calculo de orcamento, pedidos, admin, estoque, configuracoes, pagamento manual e recuperacao de senha.

Veja `API/README.md` para criar o banco MySQL local, aplicar migrations e configurar VPS/Hostinger.
