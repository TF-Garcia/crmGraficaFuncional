# PrintFlow API

API real do CRM da grafica, criada seguindo a estrutura do BarberStyle: ASP.NET Core, controllers REST, JWT, Entity Framework, migrations, seed inicial e Mercado Pago preparado para pagamento real.

## Banco local MySQL

Use sempre um usuario especifico da grafica, separado do `root`. Se ele ainda nao existir, crie o banco e o usuario no MySQL local:

```sql
CREATE DATABASE printflow_crm_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'printflow_user'@'localhost' IDENTIFIED BY 'troque-esta-senha';
GRANT ALL PRIVILEGES ON printflow_crm_dev.* TO 'printflow_user'@'localhost';
FLUSH PRIVILEGES;
```

Se voce ja tem esse usuario criado, basta ajustar `API/PrintFlowApi/appsettings.Development.json` com o nome e a senha reais dele:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=printflow_crm_dev;User=SEU_USUARIO_DA_GRAFICA;Password=SUA_SENHA;TreatTinyAsBoolean=true;"
```

Depois rode:

```bash
cd API/PrintFlowApi
dotnet ef database update
dotnet run
```

Tambem deixei um script SQL idempotente em `API/PrintFlowApi/schema.mysql.sql` caso voce prefira aplicar o schema pelo MySQL Workbench ou painel da VPS.

A API sobe com seed inicial:

- Admin: `admin@printflowpro.com.br` / `Admin@123456`
- Cliente: `contato@studiobella.com.br` / `Cliente@123456`

## Variaveis para VPS Hostinger

No Linux, prefira variaveis de ambiente em vez de editar `appsettings.json`:

```bash
ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=printflow_crm;User=printflow_user;Password=SENHA_FORTE;TreatTinyAsBoolean=true;"
Jwt__Issuer="PrintFlowApi"
Jwt__Audience="PrintFlowFrontend"
Jwt__Secret="UMA_CHAVE_LONGA_COM_MAIS_DE_32_CARACTERES"
Cors__Origins__0="https://SEU-FRONT.vercel.app"
MercadoPago__AccessToken="APP_USR-..."
MercadoPago__PublicBaseUrl="https://api.seu-dominio.com.br"
MercadoPago__FrontendBaseUrl="https://SEU-FRONT.vercel.app"
MercadoPago__UseSandbox="false"
```

## Vercel

No frontend, use uma variavel como:

```bash
VITE_API_BASE_URL=https://api.seu-dominio.com.br
```

## Rotas principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/catalogo/produtos`
- `POST /api/orcamentos/calcular`
- `POST /api/pedidos`
- `GET /api/pedidos/meus`
- `POST /api/pagamentos/mercado-pago/preferencia`
- `POST /api/pagamentos/mercado-pago/webhook`
- `GET /api/admin/dashboard`
- `GET /api/admin/pedidos`
- `GET /api/admin/clientes`
- `GET /api/admin/estoque`

## Pagamentos reais

O fluxo Mercado Pago ja cria preferencia, salva `ProviderReference`, redireciona para checkout e recebe webhook. Para producao, falta apenas preencher `MercadoPago__AccessToken`, apontar `PublicBaseUrl` para a API publica com HTTPS e cadastrar o webhook no painel do Mercado Pago.
