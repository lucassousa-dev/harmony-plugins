# Harmony Plugins

Loja virtual para venda e distribuição de plugins para jogos, com versionamento dos arquivos e pagamento integrado ao Mercado Pago.

Projeto desenvolvido por mim (Lucas Sousa) como Trabalho de Graduação do curso de Análise e Desenvolvimento de Sistemas da Fatec Itu.

## Situação

Em desenvolvimento. A pesquisa, os requisitos e a modelagem estão concluídos, e a implementação está no início.

Hoje o repositório tem:

- a API em ASP.NET Core com cadastro e login (JWT);
- o modelo de dados completo, conforme o dicionário de dados do TG, com as migrations do SQL Server;
- o ambiente de desenvolvimento em Docker (API, SQL Server e Azurite);
- testes automatizados da autenticação.

As telas, o catálogo, os pedidos, o pagamento e os downloads ainda não foram implementados.

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

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/health` | Verifica se a API está no ar |
| POST | `/api/auth/register` | Cadastra um cliente (senha de 15 a 64 caracteres) |
| POST | `/api/auth/login` | Autentica e devolve o token JWT |

Exemplos de requisição estão em `HarmonyPlugins.Api/HarmonyPlugins.Api.http`.

## Como executar

### Com Docker

Com o Docker Desktop aberto, copie `.env.example` para `.env`, troque a senha do SQL Server e a chave JWT, e rode:

```bash
docker compose up -d --build
```

As migrations são aplicadas na inicialização. A API fica em `http://localhost:8080` e o Swagger em `http://localhost:8080/swagger`.

Para parar o ambiente, use `docker compose down`. Os dados ficam em volumes do Docker; acrescente `-v` para apagá-los.

### Sem Docker

É preciso ter o .NET 10 SDK e um SQL Server. Informe a connection string e a chave JWT (pelo menos 32 bytes) por variáveis de ambiente ou em um `appsettings.Development.json`, que não é versionado:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=HarmonyPluginsDb;Trusted_Connection=True;TrustServerCertificate=True;"
$env:Jwt__Key = "troque-por-uma-chave-secreta-com-pelo-menos-32-bytes"
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

1. Perfis e autorização, plugins, categorias e jogos, catálogo e publicação de versões com upload.
2. Carrinho, pedidos, pagamento pelo Mercado Pago, webhooks, aquisições e plugins gratuitos.
3. Downloads autorizados, biblioteca do cliente, painel administrativo, moderação e auditoria.
4. Testes, ajustes de segurança e versão de demonstração.
