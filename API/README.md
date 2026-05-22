# PrintFlow API

API do CRM da grafica em ASP.NET Core, JWT, Entity Framework, SQLite, seed inicial, recuperacao de senha por email e pagamentos Mercado Pago.

## Banco SQLite

Este modelo/demo nao usa MySQL. A API cria o arquivo SQLite automaticamente no startup:

```json
"DefaultConnection": "Data Source=data/printflow-demo-dev.db"
```

Para rodar local:

```bash
cd API/PrintFlowApi
dotnet run
```

A API sobe com seed inicial:

- Admin: `admin@printflowpro.com.br` / `Admin@123456`
- Cliente: `contato@studiobella.com.br` / `Cliente@123456`

## Vercel

O frontend publicado precisa apontar para a API publica da VPS. No painel da Vercel, configure:

```bash
VITE_API_BASE_URL=http://SEU_IP_OU_DOMINIO:8080
```

Depois faca um novo deploy do frontend. Se essa variavel ficar vazia ou apontar para o dominio da Vercel, o navegador tentara acessar `/api/...` dentro do proprio Vercel e retornara `404`.

## Docker Compose na VPS

Na VPS:

```bash
cp .env.vps.example .env.vps
nano .env.vps
docker compose --env-file .env.vps up -d --build
```

O arquivo SQLite fica persistido no volume Docker `printflow_sqlite_data`, montado em `/app/data`.

Ver logs:

```bash
docker compose --env-file .env.vps logs -f api
```

Parar sem apagar dados:

```bash
docker compose --env-file .env.vps down
```

Apagar o banco demo/volume:

```bash
docker compose --env-file .env.vps down -v
```

## Mercado Pago

No `.env.vps`, preencha:

```bash
MERCADOPAGO_PUBLIC_KEY=TEST-sua-public-key
MERCADOPAGO_ACCESS_TOKEN=TEST-seu-access-token
MERCADOPAGO_NOTIFICATION_URL=http://SEU_IP_OU_DOMINIO:8080/api/pagamentos/webhook/mercado-pago
```

Pix e cartao ficam pendentes ate aprovacao do Mercado Pago. Pagamento no balcao entra como pago imediatamente.
