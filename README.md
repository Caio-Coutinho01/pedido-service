# Pedido Service

Serviço responsável por receber, processar e disponibilizar pedidos, com cálculo de imposto, validações e integração com sistemas externos. Desenvolvido com foco em Clean Architecture, SOLID, boas práticas e preparado para alto volume de dados.

---

## Tecnologias Utilizadas

- .NET 8
- Entity Framework Core 9
- MediatR
- AutoMapper
- FluentValidation
- Serilog
- Feature Management
- Polly (Retry + Circuit Breaker)
- Hangfire (Jobs recorrentes)
- XUnit + NSubstitute + FluentAssertions
- Arquitetura DDD + Clean Code + SOLID

## Ferramentas Complementares

* Docker
* Testcontainers

---

## Funcionalidades

- Recebimento e persistência de pedidos
- Cálculo de imposto com base em feature flag
- Cancelamento de pedidos com justificativa
- Listagem e consulta por status e ID
- Envio de pedidos ao sistema externo (Sistema B) via Event-Driven
- Reprocessamento automático de pedidos com falha
- Controle de tentativas e status por pedido
- Logs persistidos no SQL Server via Serilog
- Monitoramento e reprocesso por Jobs (Hangfire)
- Documentação automática via Swagger

---

## Estrutura do Projeto

- **Pedido.API:** Camada de apresentação, onde são expostos os endpoints HTTP (Swagger incluso).
- **Pedido.Application:** Regras de negócio, validações, serviços de aplicação, eventos e handlers.
- **Pedido.Domain:** Entidades ricas, objetos de valor, enums e regras centrais de domínio.
- **Pedido.Infrastructure:** Persistência com EF Core, Serilog, políticas de retry (Polly), e integrações externas (simulação Sistema B).
- **Pedido.Tests:** Testes unitários com mocks e testes de integração utilizando containers Docker.

---

## Endpoints Disponíveis

- **POST** `/api/pedidos` → Criar um novo pedido
- **GET** `/api/pedidos/{id}` → Consultar pedido por ID
- **GET** `/api/pedidos?status={status}` → Listar pedidos por status
- **POST** `/api/pedidos/{id}/cancelar` → Cancelar pedido com justificativa
- **POST** `/api/pedidos/enviar-todos-elegiveis` → Reprocessar pedidos com status `Criado` ou `ErroEnvio`

## Exemplos de Request/Response

##### Criar Pedido

###### Request:

```json
{
  "pedidoId": 1,
  "clienteId": 123,
  "itens": [
    {
      "produtoId": 101,
      "quantidade": 2,
      "valor": 50
    }
  ]
}
```

---

###### Response:

```json
{
  "id": 1,
  "status": "Criado"
}
```

---

## Reprocessamento Automático (Hangfire + Retry)
Um job recorrente executado a cada 5 minutos (via Hangfire) reprocessa todos os pedidos com status Criado ou ErroEnvio.
Cada pedido tem controle individual de tentativas (TentativasEnvio) com limite configurável via appsettings.json.
Se o pedido falhar 3 vezes consecutivas, é marcado como ErroEnvio, evitando loops infinitos.

## Logs e Monitoramento

O projeto utiliza **Serilog** configurado para armazenar logs de erros e avisos em uma tabela SQL Server (`Logs`). Eventos como falha de envio, circuit breaker, e erros de infraestrutura são rastreados com detalhamento para análise posterior.

---

## Como Executar

### Pré-requisitos:

* .NET SDK 8+
* SQL Server
* Visual Studio / VSCode
* Docker

### Execução:

`git clone https://github.com/Caio-Coutinho01/pedido-service.git`

###### Restore

dotnet restore

###### Aplicar Migrations (caso use SQL Server), recomendo abrir o package manager console

`dotnet ef database update --project Pedido.Infrastructure --startup-project Pedido.API`

###### Acessar pelo navegador (local):

`https://localhost:7292/swagger`

---

## Testes

Execute os testes com:

`dotnet test`

### Testes com Testcontainers

Para executar os testes de integração com banco real em container:

- Verifique se o Docker está em execução
- Execute normalmente com `dotnet test` (os testes de integração usarão o container configurado)

Cenários testes unitarios cobertos:

* Criação de pedidos
* Envio de pedidos criados ao outro sistema (B)
* Pedido duplicado
* Cancelamento de pedidos
* Consulta por ID
* Listagem por status

Cenários testes de integração cobertos:

* Cancelamento de pedidos
* Consulta por ID
* Criação de pedidos
* Envio de pedidos criados ao outro sistema (B)
* Listagem de pedidos por status

---

## Feature Flags

O tipo de cálculo de imposto pode ser alternado com o uso de feature flags.

Exemplo de configuração no `appsettings.json`:

```json
"FeatureManagement": {
  "UsarNovaRegraImposto": true
}
```

* `false`: Aplica a regra antiga → 30% de imposto
* `true`: Aplica a nova regra da reforma tributária → 20%

---


## Boas Práticas Aplicadas

* Clean Architecture (camadas separadas e independentes)
* Princípios SOLID, DRY e YAGNI
* Object Calisthenics (aplicado sempre que possível)
* Git Flow e commits semânticos
* Tratamento de exceções com logs detalhados
* Encapsulamento e uso de objetos de valor no domínio
* Separação clara de responsabilidades entre camadas
* Testes focados em comportamento e regras de negócio
* Projeto preparado para escalar, com feature flags e arquitetura orientada a eventos
* Políticas de retry e circuit breaker com Polly
* Controle de falhas e reprocessos com Hangfire
