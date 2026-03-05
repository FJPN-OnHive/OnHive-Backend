# OnHive Backend

Backend da plataforma **OnHive**: API monolítica em .NET que centraliza usuários, inquilinos (multi-tenant), catálogo, cursos, alunos, professores, pedidos, pagamentos, faturas, certificados, eventos, mensagens, armazenamento e demais domínios do ecossistema.

## Tecnologias

- **.NET 10**
- **ASP.NET Core** (minimal APIs / endpoints)
- **MongoDB**
- **Docker** (imagem Linux para build e execução)
- **OpenAPI / Scalar** para documentação da API

## Estrutura do repositório

| Pasta / conceito | Descrição |
|------------------|-----------|
| **OnHive.BackEnd.Api** | API principal que agrega todos os serviços |
| **Services/** | Domínios em formato *Api / Services / Repositories* (e testes), ex.: Catalog, Courses, Students, Teachers, Orders, Payments, Invoices, Certificates, Events, Messages, Storages, Search, Tenants, Users, etc. |
| **Services/OnHive.Domain** | Contratos, entidades e abstrações compartilhadas entre os serviços |
| **Libraries/** | Bibliotecas compartilhadas: Authorization, Configuration, Database, Enrich, HealthCheck, Observability, WebExtensions |
| **Client/OnHive.Admin** | Aplicação administrativa (Blazor/WebAssembly) referenciada pela API |

Cada domínio segue o padrão: **Api** (endpoints), **Services** (regras de negócio), **Repositories** (persistência) e **Tests**.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (local ou via Docker)
- Opcional: [Docker](https://docs.docker.com/get-docker/) para rodar a API em container

## Executando localmente

1. Clone o repositório e entre na pasta do projeto:
   ```bash
   cd OnHive-Backend
   ```

2. Restaure e execute a API:
   ```bash
   dotnet restore OnHive.BackEnd.Api.sln
   dotnet run --project OnHive.BackEnd.Api
   ```
   Por padrão (perfil **http** em `launchSettings.json`), a API fica em **http://localhost:4000**.

3. Configure o MongoDB (por exemplo em `appsettings.json` ou User Secrets) com algo como:
   - `MongoDbSettings__ConnectionString`: string de conexão
   - `MongoDbSettings__DataBase`: ex. `onhive`

Para HTTPS e outras variáveis de ambiente, use o perfil **https** ou ajuste em **Properties/launchSettings.json**.

## Executando com Docker

Na raiz do repositório (onde está o `Dockerfile` do `OnHive.BackEnd.Api`):

```bash
docker build -t onhive-backend -f OnHive.BackEnd.Api/Dockerfile .
docker run -p 4000:4000 -p 4001:4001 --env-file .env onhive-backend
```

A API expõe as portas **4000** (HTTP) e **4001** (HTTPS). Passe as variáveis necessárias (ex.: MongoDB) via `--env-file` ou `-e`.

## Documentação da API

Com a API em execução:

- **OpenAPI (Swagger)** e **Scalar** estão configurados; acesse a URL de documentação configurada no projeto (geralmente algo como `/scalar/v1` ou rota equivalente ao `MapScalarApiReference()`).

## Testes

```bash
dotnet test OnHive.BackEnd.Api.sln
```

## Documentação e links

| Recurso | Link |
|--------|------|
| **Acordos técnicos** (guidelines, .NET 10+, convenções de código) | [DevGuideLines.md](./docs/DevGuideLines.md) |
| **Contratos de API** (resposta padrão, paginação, filtros, patch) | [GeneralContracts.md](./docs/GeneralContracts.md) |
| **Front-End** | [OnHive-Frontend](https://github.com/FJPN-OnHive/OnHive-Frontend) |
| **Instalação** | [OnHive-Instalation](https://github.com/FJPN-OnHive/OnHive-Instalation) |

## Arquitetura macro

![Arquitetura Macro](./docs/OnHiveMacroArchitecture.jpg)
