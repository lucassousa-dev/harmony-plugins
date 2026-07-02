# Harmony Plugins

> Plataforma open-source para comercialização, versionamento e distribuição segura de plugins digitais.

![Status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)
![C#](https://img.shields.io/badge/C%23-.NET-512BD4)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-5C2D91)
![SQL Server](https://img.shields.io/badge/SQL%20Server-banco%20relacional-CC2927)

## Status do projeto

🚧 **Em desenvolvimento — fase inicial do back-end**

Este repositório está sendo construído gradualmente como parte do meu **Trabalho de Graduação (TG)** em Análise e Desenvolvimento de Sistemas na Fatec Itu.

O projeto ainda está em desenvolvimento. As funcionalidades descritas neste README representam o **escopo planejado para o MVP** e serão implementadas, testadas e documentadas progressivamente ao longo do desenvolvimento.

## Sobre o projeto

O **Harmony Plugins** é uma plataforma web open-source para venda, distribuição e versionamento de plugins digitais.

A proposta é criar uma base que permita que administradores cadastrem plugins gratuitos ou pagos, gerenciem versões, pedidos, pagamentos e downloads, enquanto clientes possam visualizar plugins, comprar produtos pagos, baixar plugins gratuitos e acessar uma biblioteca com os plugins adquiridos.

Além do objetivo acadêmico, o projeto também será usado como meu principal projeto de portfólio em back-end, aplicando conceitos de desenvolvimento de APIs REST, autenticação, banco de dados, regras de negócio e documentação técnica.

## Problema que o projeto busca resolver

Desenvolvedores independentes que comercializam plugins digitais geralmente precisam lidar com várias etapas separadas:

- cadastro e organização dos plugins;
- controle de versões e changelogs;
- disponibilização de arquivos para download;
- controle de pagamentos;
- liberação de acesso após compra;
- histórico de pedidos e downloads;
- área para o cliente acessar novamente o que adquiriu.

Quando esse processo é feito manualmente ou usando ferramentas desconectadas, a operação pode se tornar difícil de manter, pouco escalável e sujeita a falhas.

O **Harmony Plugins** busca centralizar esse fluxo em uma plataforma própria, com foco inicial em um MVP funcional e bem documentado.

## Objetivo acadêmico

Desenvolver uma plataforma web para comercialização, versionamento e distribuição segura de plugins digitais, contemplando autenticação, controle de acesso, gerenciamento de produtos digitais, pedidos, pagamento em ambiente de testes, biblioteca do cliente e downloads autorizados.

## Objetivo como portfólio

Este projeto tem como objetivo demonstrar evolução prática em desenvolvimento back-end com **C#/.NET**, por meio de um sistema com regras de negócio reais e maior complexidade do que um CRUD simples.

Durante o desenvolvimento, o foco será registrar decisões técnicas, manter commits organizados, documentar a arquitetura e evoluir o projeto de forma incremental.

## Escopo do MVP

O MVP será focado em uma loja própria de plugins, sem marketplace multi-vendedor nesta primeira versão.

### Incluído no MVP

- Autenticação de usuários.
- Perfis de acesso: cliente e administrador.
- Cadastro e gerenciamento de plugins digitais.
- Categorias de plugins.
- Controle de versões dos plugins.
- Plugins gratuitos e pagos.
- Criação de pedidos.
- Integração com pagamento em ambiente sandbox.
- Liberação de download após pagamento aprovado.
- Biblioteca do cliente com plugins adquiridos.
- Registro de downloads.
- Painel administrativo básico.
- Documentação técnica do projeto.

### Fora do MVP inicial

- Marketplace aberto para múltiplos vendedores.
- Repasse financeiro para terceiros.
- Sistema de afiliados.
- Cupons de desconto.
- Assinaturas recorrentes.
- Integração direta com servidores de jogo.
- Sistema avançado de licenças in-game.
- Antipirataria avançado.
- Avaliações e comentários públicos.

Esses itens podem ser considerados como evoluções futuras após a entrega da primeira versão funcional.

## Funcionalidades planejadas

### Autenticação e usuários

- Cadastro de usuário.
- Login.
- Hash de senha.
- Autenticação com JWT.
- Perfil de cliente.
- Perfil de administrador.
- Proteção de rotas por perfil.

### Plugins

- Cadastro de plugins.
- Edição de plugins.
- Listagem pública de plugins publicados.
- Busca por nome ou slug.
- Filtro por categoria.
- Filtro por tipo: gratuito ou pago.
- Controle de status: rascunho, publicado ou inativo.

### Categorias

- Cadastro de categorias.
- Edição de categorias.
- Listagem de categorias.
- Desativação de categorias.

### Versões de plugins

- Cadastro de novas versões.
- Controle de número da versão.
- Changelog.
- Arquivo do plugin.
- Definição de versão atual.
- Histórico de versões.

### Pedidos e pagamentos

- Criação de pedidos para plugins pagos.
- Registro dos itens do pedido.
- Integração com gateway de pagamento em ambiente sandbox.
- Controle de status de pagamento.
- Webhook para confirmação de pagamento.
- Liberação automática do plugin após pagamento aprovado.

### Biblioteca do cliente

- Listagem de plugins comprados.
- Listagem de plugins gratuitos baixados.
- Acesso às versões disponíveis.
- Download de plugins autorizados.

### Downloads

- Download de plugins gratuitos por usuários autenticados.
- Download de plugins pagos somente após pagamento aprovado.
- Registro de histórico de downloads.
- Bloqueio de acesso indevido a arquivos pagos.

### Painel administrativo

- Visualização de usuários.
- Visualização de pedidos.
- Visualização de pagamentos.
- Visualização de downloads.
- Métricas básicas da plataforma.
- Plugins mais baixados.
- Receita aprovada.
- Pedidos pendentes e aprovados.

## Tecnologias planejadas

### Back-end

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT
- Swagger / OpenAPI
- xUnit para testes automatizados

### Ferramentas

- Visual Studio
- SQL Server Management Studio
- Git e GitHub
- Postman / Swagger
- Markdown para documentação

### Pagamento

A integração de pagamento será implementada inicialmente em ambiente de testes/sandbox.

Opções consideradas:

- Stripe
- Mercado Pago

## Arquitetura planejada

A API será desenvolvida com separação de responsabilidades, evitando concentrar regras de negócio diretamente nos controllers.

Estrutura planejada para o back-end:

```text
HarmonyPlugins.Api
│
├── Controllers
├── Application
│   ├── DTOs
│   ├── Services
│   └── Interfaces
├── Domain
│   ├── Entities
│   ├── Enums
│   └── Exceptions
├── Infrastructure
│   ├── Data
│   ├── Repositories
│   ├── Migrations
│   └── Integrations
│       └── Payments
├── Middlewares
├── Extensions
└── Program.cs
```

Projeto de testes planejado:

```text
HarmonyPlugins.Tests
```

## Principais entidades planejadas

- User
- Plugin
- PluginCategory
- PluginVersion
- Order
- OrderItem
- Payment
- CustomerLibrary
- DownloadLog
- AuditLog

## Exemplos de regras de negócio

- Um cliente só pode baixar um plugin pago se o pagamento estiver aprovado.
- Um plugin gratuito pode ser baixado por usuários autenticados.
- Apenas administradores podem cadastrar plugins, versões e categorias.
- Apenas plugins publicados devem aparecer na listagem pública.
- Um plugin pago deve ter preço maior que zero.
- Um plugin gratuito deve ter preço igual a zero.
- Cada plugin pode possuir várias versões.
- Apenas uma versão deve ser definida como versão atual.
- O sistema deve registrar downloads realizados.
- O cliente só pode visualizar sua própria biblioteca.

## O que estou praticando com este projeto

Este projeto está sendo desenvolvido para consolidar conhecimentos importantes para atuação como desenvolvedor back-end júnior.

Durante o desenvolvimento, estou praticando:

- criação de APIs REST com ASP.NET Core;
- organização de projetos reais;
- separação entre controller, service, DTO, entidade e repositório;
- criação de entidades e relacionamentos;
- uso do Entity Framework Core;
- criação e aplicação de migrations;
- autenticação com JWT;
- autorização baseada em perfis;
- tratamento de erros;
- validações de entrada;
- integração com serviços externos;
- testes automatizados;
- documentação de API;
- versionamento com Git;
- escrita de commits mais profissionais;
- planejamento de features antes da implementação.

## Roadmap

### Fase 1 — Estrutura inicial da API

- [ ] Planejamento técnico inicial.
- [ ] Criação da solution e estrutura base.
- [ ] Configuração do Swagger.
- [ ] Configuração do Entity Framework Core.
- [ ] Criação das entidades principais.
- [ ] Criação da primeira migration.

### Fase 2 — Autenticação e usuários

- [ ] Cadastro de usuários.
- [ ] Login com JWT.
- [ ] Controle de acesso por perfil.

### Fase 3 — Plugins, categorias e versões

- [ ] CRUD de categorias.
- [ ] CRUD de plugins.
- [ ] Versionamento de plugins.
- [ ] Upload de arquivo do plugin.
- [ ] Definição de versão atual.

### Fase 4 — Pedidos, pagamentos e downloads

- [ ] Criação de pedidos.
- [ ] Integração com pagamento em sandbox.
- [ ] Webhook de pagamento.
- [ ] Biblioteca do cliente.
- [ ] Download autorizado.

### Fase 5 — Administração, testes e documentação

- [ ] Painel administrativo.
- [ ] Testes automatizados.
- [ ] Documentação final da API.

## Como executar o projeto

> Esta seção será atualizada conforme o desenvolvimento avançar.

Pré-requisitos esperados:

- .NET SDK instalado.
- SQL Server instalado.
- Visual Studio ou VS Code.
- Git instalado.

Comandos iniciais previstos:

```bash
git clone https://github.com/lucassousa-dev/harmony-plugins.git
cd harmony-plugins
dotnet restore
dotnet build
dotnet run --project HarmonyPlugins.Api
```

Após executar a API, a documentação Swagger deverá ficar disponível em uma URL local semelhante a:

```text
https://localhost:porta/swagger
```

## Documentação

A documentação técnica será mantida na pasta `docs/` conforme o projeto avançar.

Arquivos planejados:

```text
docs/
├── requisitos.md
├── modelagem.md
├── arquitetura.md
├── banco-de-dados.md
├── endpoints.md
├── pagamentos.md
├── cronograma.md
└── decisoes-tecnicas.md
```

## Observação

Este projeto está sendo desenvolvido com finalidade acadêmica e profissional.

O README descreve o escopo planejado para o MVP e será atualizado conforme as funcionalidades forem implementadas. A proposta é evoluir o projeto de forma incremental, mantendo o código, a documentação e o histórico de commits alinhados ao desenvolvimento real.
