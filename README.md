# Harmony Plugins

Loja virtual para venda e distribuição de plugins para jogos, com versionamento dos arquivos e pagamento integrado ao Mercado Pago.

Projeto desenvolvido por mim (Lucas Sousa) como Trabalho de Graduação do curso de Análise e Desenvolvimento de Sistemas da Fatec Itu.

## Situação

Em desenvolvimento. A pesquisa, os requisitos e a modelagem estão concluídos, e a implementação está em andamento.

Hoje o repositório tem:

- a API em ASP.NET Core com cadastro, login, renovação e encerramento de sessão (JWT de 15 minutos e token de renovação de 7 dias);
- autorização por perfil, com negação como padrão, e limite de tentativas de login;
- a área administrativa de jogos, categorias e plugins, com publicação de versões e upload dos arquivos para o Blob Storage;
- o catálogo público com busca, filtros e página do plugin;
- log de auditoria das operações sensíveis;
- o modelo de dados completo, conforme o dicionário de dados do TG, com as migrations do SQL Server;
- o ambiente de desenvolvimento em Docker (API, SQL Server e Azurite);
- testes automatizados.

As telas, os pedidos, o pagamento, os downloads e a moderação ainda não foram implementados.

## Sobre o projeto

Quem vende plugins costuma juntar várias ferramentas separadas: uma para receber o pagamento, outra para guardar os arquivos e um controle manual de quem comprou o quê. O Harmony Plugins reúne esse fluxo em uma loja própria. O administrador cadastra os plugins e publica as versões. O cliente compra ou obtém plugins gratuitos e baixa, a qualquer momento, todas as versões dos plugins que adquiriu, sem pagar de novo pelas atualizações.

Fazem parte do escopo:

- cadastro e login de clientes e administradores;
- catálogo com busca e filtros por jogo, categoria, tipo e preço;
- carrinho, pedidos e pagamento pelo Mercado Pago (Checkout Pro, em ambiente de testes);
- plugins gratuitos, obtidos sem pedido;
- biblioteca do cliente com download por URL temporária;
- área administrativa com plugins, versões, moderação, usuários, vendas, downloads e auditoria.

Ficam de fora a venda por desenvolvedores externos (marketplace), a emissão de notas fiscais, pagamentos reais em produção, a instalação automática nos jogos e aplicativos móveis.

## Tecnologias

| Parte | Tecnologia |
|---|---|
| API | C#, .NET 10, ASP.NET Core Web API |
| Banco de dados | SQL Server com Entity Framework Core 10 |
| Autenticação | JWT (HMAC-SHA256) e senhas com BCrypt |
| Arquivos | Azure Blob Storage (Azurite no ambiente local) |
| Pagamento | Mercado Pago Checkout Pro |
| Interface web | Angular (ainda não iniciada) |
| Testes | xUnit |
| Ambiente | Docker e Docker Compose |
| Documentação da API | OpenAPI e Swagger UI |

## Estrutura

```text
HarmonyPlugins.Api/
├── Controllers/          endpoints HTTP
├── Application/          DTOs, interfaces e serviços
├── Domain/               entidades e enums
├── Infrastructure/Data/  DbContext e migrations
└── Program.cs
HarmonyPlgins.Tests/      testes automatizados
docker-compose.yml
```

## Endpoints

Públicos:

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/health` | Verifica se a API está no ar |
| POST | `/api/auth/register` | Cadastra um cliente (senha de 15 a 64 caracteres) |
| POST | `/api/auth/login` | Autentica e devolve o token de acesso e o de renovação (5 tentativas por minuto) |
| POST | `/api/auth/refresh` | Troca o token de renovação por um novo par de tokens |
| POST | `/api/auth/logout` | Revoga o token de renovação |
| GET | `/api/catalog/plugins` | Lista os plugins publicados (`search`, `gameId`, `categoryId`, `type`, `minPrice`, `maxPrice`, `page`, `pageSize`) |
| GET | `/api/catalog/plugins/{slug}` | Página do plugin com o histórico de versões |
| GET | `/api/catalog/games` | Jogos ativos, para os filtros |
| GET | `/api/catalog/categories` | Categorias ativas, para os filtros |

Administrador:

| Método | Rota | Descrição |
|---|---|---|
| GET, POST | `/api/admin/games` | Lista e cadastra jogos |
| PUT | `/api/admin/games/{id}` | Edita um jogo |
| GET, POST | `/api/admin/categories` | Lista e cadastra categorias |
| PUT | `/api/admin/categories/{id}` | Edita uma categoria |
| GET, POST | `/api/admin/plugins` | Lista e cadastra plugins (novos ficam como rascunho) |
| GET, PUT | `/api/admin/plugins/{id}` | Detalha e edita um plugin |
| POST | `/api/admin/plugins/{id}/publish` | Publica (exige versão atual ativa) |
| POST | `/api/admin/plugins/{id}/unpublish` | Volta para rascunho |
| POST | `/api/admin/plugins/{id}/versions` | Publica uma versão (multipart, `.zip` ou `.jar` até 100 MB) |
| POST | `/api/admin/plugins/{id}/versions/{versionId}/current` | Define a versão atual |

As demais rotas exigem o token no cabeçalho `Authorization: Bearer`. Os erros seguem o formato Problem Details. Exemplos de requisição estão em `HarmonyPlugins.Api/HarmonyPlugins.Api.http`.

## Como executar

### Com Docker

Com o Docker Desktop aberto, copie `.env.example` para `.env`, troque a senha do SQL Server, a chave JWT e os dados do primeiro administrador, e rode:

```bash
docker compose up -d --build
```

As migrations são aplicadas na inicialização, e o administrador é criado se ainda não houver nenhum (o cadastro público só cria clientes). A API fica em `http://localhost:8080` e o Swagger em `http://localhost:8080/swagger`.

Para parar o ambiente, use `docker compose down`. Os dados ficam em volumes do Docker; acrescente `-v` para apagá-los.

### Sem Docker

É preciso ter o .NET 10 SDK, um SQL Server e, para o upload de versões, o Azurite. Informe as configurações por variáveis de ambiente ou em um `appsettings.Development.json`, que não é versionado:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=HarmonyPluginsDb;Trusted_Connection=True;TrustServerCertificate=True;"
$env:ConnectionStrings__BlobStorage = "UseDevelopmentStorage=true"
$env:Jwt__Key = "troque-por-uma-chave-secreta-com-pelo-menos-32-bytes"
$env:Admin__Email = "admin@example.com"
$env:Admin__Password = "troque-por-uma-senha-longa"
```

```bash
dotnet ef database update --project HarmonyPlugins.Api
dotnet run --project HarmonyPlugins.Api
```

O Swagger fica em `http://localhost:5224/swagger`.

### Testes

```bash
dotnet test
```

## Próximos passos

1. Carrinho, pedidos, pagamento pelo Mercado Pago, webhooks, aquisições e plugins gratuitos.
2. Downloads autorizados, biblioteca do cliente, painel administrativo, moderação e consulta da auditoria.
3. Testes, ajustes de segurança e versão de demonstração.
