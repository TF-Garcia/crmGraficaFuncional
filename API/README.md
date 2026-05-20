# PrintFlow API

API real do CRM da grafica, criada seguindo a estrutura do BarberStyle: ASP.NET Core, controllers REST, JWT, Entity Framework, migrations, seed inicial, recuperacao de senha por email e pagamentos preparados para integracao futura.

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
FrontendUrl="https://SEU-FRONT.vercel.app"
Smtp__Host="smtp.hostinger.com"
Smtp__Port="465"
Smtp__User="suporte@seudominio.com"
Smtp__Password="SENHA_SMTP"
Smtp__FromEmail="suporte@seudominio.com"
Smtp__FromName="CRM Grafica Modelo"
Smtp__EnableSsl="true"
```

## Vercel

No frontend, use uma variavel como:

```bash
VITE_API_BASE_URL=https://api.seu-dominio.com.br
```

## Rotas principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `GET /api/perfil`
- `PUT /api/perfil`
- `GET /api/catalogo/produtos`
- `POST /api/orcamentos/calcular`
- `POST /api/orcamentos`
- `GET /api/orcamentos/meus`
- `POST /api/orcamentos/{id}/converter`
- `POST /api/pedidos`
- `GET /api/pedidos/meus`
- `POST /api/pagamentos/{orderId}/confirmar-manual`
- `GET /api/admin/dashboard`
- `GET /api/admin/pedidos`
- `GET /api/admin/clientes`
- `GET /api/admin/estoque`
- `POST /api/admin/estoque/movimentacoes`
- `GET /api/admin/configuracoes`
- `PUT /api/admin/configuracoes`

## Pagamentos

Nao ha gateway ativo nesta etapa. A tabela de pagamentos existe e o admin/financeiro pode confirmar pagamento manualmente. Pix/cartao ficam apenas como metodos preparados para integracao futura.
