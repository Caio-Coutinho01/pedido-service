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

## Como Executar

### Pré-requisitos:

* .NET SDK 8+
* SQL Server
* Visual Studio / VSCode

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

Cenários cobertos:

* Criação de pedidos
* Pedido duplicado
* Cancelamento de pedidos
* Consulta por ID
* Listagem por status

---

## Feature Flags

A feature de cálculo de imposto pode ser controlada via `appsettings.json`:

`"FeatureManagement": {   "UsarNovaRegraImposto": true }`

## Boas Práticas Aplicadas

* Clean Architecture (camadas separadas e independentes)
* Princípios SOLID, DRY e YAGNI
* Object Calisthenics
* Git Flow e commits semânticos
* Tratamento de exceções com logs detalhados

---

## Requisitos Atendidos

* Divisão em camadas (API, Domain, Application, Infrastructure)
* Serilog configurado e persistido em banco
* Clean Code + SOLID
* Git Flow + Commit semântico
* Testes Unitários com cobertura significativa
* MediatR + Event-Driven
* Feature Flag
* Documentação via Swagger
