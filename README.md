# Pedido Service

Serviço responsável por receber, processar e disponibilizar pedidos, com cálculo de imposto, validações e integração com sistemas externos. Desenvolvido com foco em Clean Architecture, SOLID, boas práticas e preparado para alto volume de dados.

---

## Tecnologias Utilizadas

* .NET 8
* Entity Framework Core 9
* MediatR
* AutoMapper
* FluentValidation
* Serilog
* Feature Management
* XUnit + NSubstitute + FluentAssertions
* Arquitetura DDD + Clean Code + SOLID

## Ferramentas Complementares

* Docker
* Testcontainers

---

## Funcionalidades

* Recebimento e persistência de pedidos
* Cálculo de imposto com base em feature flag
* Cancelamento de pedidos com justificativa
* Listagem e filtragem por status
* Consulta de pedido por ID
* Envio para sistema externo (simulado) via MediatR (Event-Driven)
* Logs de erros persistidos no SQL Server via Serilog
* Documentação automática via Swagger

---

## Estrutura do Projeto

* **Pedido.API:** Responsável por receber as requisições HTTP e expor os endpoints da aplicação (ex: criação, consulta, listagem de pedidos).
* **Pedido.Application:** Onde ficam as regras de negócio, validações, serviços de aplicação e o uso do MediatR pra orquestrar as ações.
* **Pedido.Domain:** Contém as entidades do sistema (Pedido, Item, etc.), enums e objetos de valor. Tudo que representa o “coração” da regra de domínio.
* **Pedido.Infrastructure:** Cuida da persistência (EF Core), configuração do Serilog, e simulação de integrações externas (como o envio para Sistema B).
* **Pedido.Tests:** Abriga todos os testes: unitários com mocks e testes de integração com containers Docker.

---

## Endpoints Disponíveis

- **POST** `/api/pedidos` → Criar um novo pedido
- **GET** `/api/pedidos/{id}` → Consultar pedido por ID
- **GET** `/api/pedidos?status={status}` → Listar pedidos por status
- **POST** `/api/pedidos/{id}/cancelar` → Cancelar pedido com justificativa
- **POST** `/api/pedidos/enviar-todos-criados` → Enviar pedidos com status Criado para sistema externo (Sistema B)

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

## Logs e Monitoramento

O projeto utiliza **Serilog** configurado para armazenar logs de erros e avisos em uma tabela SQL Server (`Logs`).

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

###### Aplicar Migrations (caso use SQL Server)

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
