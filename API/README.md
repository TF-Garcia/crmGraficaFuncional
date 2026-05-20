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

## Docker Compose na VPS

O modo recomendado na Hostinger VPS e subir API + MySQL juntos com Docker Compose. O MySQL fica em um volume persistente chamado `printflow_mysql_data`, e a API acessa o banco pelo host interno `mysql`.

Na VPS:

```bash
cp .env.vps.example .env.vps
nano .env.vps
docker compose --env-file .env.vps up -d --build
```

Ver logs:

```bash
docker compose --env-file .env.vps logs -f api
docker compose --env-file .env.vps logs -f mysql
```

Parar sem apagar dados:

```bash
docker compose --env-file .env.vps down
```

Apagar banco/volume, cuidado:

```bash
docker compose --env-file .env.vps down -v
```

Como a API executa migrations no startup, um banco vazio sera estruturado automaticamente e recebera o seed inicial.

## Docker manual da API

Build da imagem a partir da raiz do repositorio:

```bash
docker build -t printflow-api .
```

Exemplo de execucao apontando para MySQL da VPS:

```bash
docker run -d \
  --name printflow-api \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=printflow_crm;User=printflow_user;Password=SENHA_FORTE;TreatTinyAsBoolean=true;" \
  -e Jwt__Issuer="PrintFlowApi" \
  -e Jwt__Audience="PrintFlowFrontend" \
  -e Jwt__Secret="UMA_CHAVE_LONGA_COM_MAIS_DE_32_CARACTERES" \
  -e Cors__Origins__0="https://SEU-FRONT.vercel.app" \
  -e FrontendUrl="https://SEU-FRONT.vercel.app" \
  -e Smtp__Host="smtp.hostinger.com" \
  -e Smtp__Port="465" \
  -e Smtp__User="suporte@seudominio.com" \
  -e Smtp__Password="SENHA_SMTP" \
  -e Smtp__FromEmail="suporte@seudominio.com" \
  -e Smtp__FromName="CRM Grafica Modelo" \
  -e Smtp__EnableSsl="true" \
  printflow-api
```

Se o MySQL estiver fora do container mas na mesma VPS, em Linux normalmente use o IP da rede Docker/host ou coloque API e MySQL em uma mesma rede Docker. Evite `localhost` dentro do container, porque ele aponta para o proprio container.

## Levar dump do banco local para a VPS

Se quiser copiar exatamente o banco local atual em vez de deixar a API criar seed do zero:

```bash
mysqldump -h localhost -P 3306 -u grafica_app -p graficaCrmModelo > graficaCrmModelo.sql
scp graficaCrmModelo.sql usuario@IP_DA_VPS:/root/
```

Na VPS, com os containers rodando:

```bash
docker exec -i printflow-mysql mysql -u"$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DATABASE" < /root/graficaCrmModelo.sql
```

Para a primeira subida, normalmente nao precisa importar dump: o schema e o seed sao aplicados pela API automaticamente.

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
