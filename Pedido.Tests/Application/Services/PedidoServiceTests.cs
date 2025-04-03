﻿using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Pedido.Application.DTOs;
using Pedido.Application.Services;
using Pedido.Domain.Entities;
using Pedido.Domain.Enums;
using Pedido.Domain.Interfaces;
using TestInfrastructure;

namespace Pedido.Tests.Application.Services
{
    public class PedidoServiceTests
    {

        public PedidoServiceTests()
        { }

        #region Criação de pedido

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CriarPedidoAsync_UtilizandoOsDoisCalculos_DeveCriarPedidoComSucesso(bool usarNovaRegraImposto)
        {
            var serviceProvider = ServiceTestFactory.BuildServiceProvider(usarNovaRegraImposto);

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();
            var repository = scope.ServiceProvider.GetRequiredService<IPedidoRepository>();

            repository.PedidoExisteAsync(Arg.Any<int>()).Returns(false);
            repository.AdicionarAsync(Arg.Any<PedidoEntity>()).Returns(Task.CompletedTask);
            repository.SalvarAlteracoesAsync().Returns(Task.CompletedTask);

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 1,
                ClienteId = 10,
                Itens = new List<ItemPedidoDTO> { new() { ProdutoId = 100, Quantidade = 2, Valor = 50 } }
            };

            var valorTotal = request.Itens.Sum(i => i.Quantidade * i.Valor);
            var impostoEsperado = usarNovaRegraImposto ? valorTotal * 0.2m : valorTotal * 0.3m;

            PedidoEntity pedidoCapturado = null!;

            repository
                .AdicionarAsync(Arg.Do<PedidoEntity>(p =>
                {
                    pedidoCapturado = p;
                }))
                .Returns(Task.CompletedTask);

            var response = await service.CriarPedidoAsync(request);

            response.Should().NotBeNull();
            response.Status.Should().Be("Criado");

            pedidoCapturado.Should().NotBeNull();
            pedidoCapturado.PedidoId.Should().Be(request.PedidoId);
            pedidoCapturado.ClienteId.Should().Be(request.ClienteId);
            pedidoCapturado.Itens.Count.Should().Be(1);

            pedidoCapturado.Imposto.Should().BeApproximately(impostoEsperado, 0.01m);

            await repository.Received(1).SalvarAlteracoesAsync();
        }

        [Fact]
        public async Task CriarPedidoAsync_PedidoDuplicado_DeveLancarExcecao()
        {
            var serviceProvider = ServiceTestFactory.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();
            var repository = scope.ServiceProvider.GetRequiredService<IPedidoRepository>();

            repository.PedidoExisteAsync(Arg.Any<int>()).Returns(true);

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 1,
                ClienteId = 10,
                Itens = new List<ItemPedidoDTO> { new() { ProdutoId = 100, Quantidade = 2, Valor = 50 } }
            };

            Func<Task> act = async () => await service.CriarPedidoAsync(request);

            var exception = await act.Should().ThrowAsync<ApplicationException>();

            exception.Which.InnerException.Should().BeOfType<InvalidOperationException>();
            exception.Which.InnerException!.Message.Should().Be("Este pedido já existe.");

            await repository.Received(0).AdicionarAsync(Arg.Any<PedidoEntity>());
            await repository.Received(0).SalvarAlteracoesAsync();
        }

        [Fact]
        public async Task CriarPedidoAsync_ErroAoSalvar_DeveLancarExcecao()
        {
            var serviceProvider = ServiceTestFactory.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();
            var repository = scope.ServiceProvider.GetRequiredService<IPedidoRepository>();

            repository.PedidoExisteAsync(Arg.Any<int>()).Returns(false);
            repository.AdicionarAsync(Arg.Any<PedidoEntity>()).Returns(Task.CompletedTask);
            repository.SalvarAlteracoesAsync().Returns<Task>(_ => throw new Exception("Falha no banco"));

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 1,
                ClienteId = 10,
                Itens = new List<ItemPedidoDTO> { new() { ProdutoId = 100, Quantidade = 2, Valor = 50 } }
            };

            Func<Task> act = async () => await service.CriarPedidoAsync(request);

            var exception = await act.Should().ThrowAsync<ApplicationException>();
            exception.Which.InnerException.Should().BeOfType<Exception>();
            exception.Which.InnerException!.Message.Should().Be("Falha no banco");

            await repository.Received(1).AdicionarAsync(Arg.Any<PedidoEntity>());
            await repository.Received(1).SalvarAlteracoesAsync();
        }

        [Fact]
        public async Task CriarPedidoAsync_MapperLancaExcecao_DeveLancarExcecao()
        {
            var mapper = Substitute.For<IMapper>();
            mapper.Map<PedidoEntity>(Arg.Any<CriarPedidoRequestDTO>()).Throws(new Exception("Erro no AutoMapper"));

            var serviceProvider = ServiceTestFactory.BuildServiceProvider(mapper: mapper);
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 10,
                ClienteId = 5,
                Itens = new List<ItemPedidoDTO> { new() { ProdutoId = 200, Quantidade = 1, Valor = 100 } }
            };

            Func<Task> act = async () => await service.CriarPedidoAsync(request);

            var exception = await act.Should().ThrowAsync<ApplicationException>();
            exception.Which.InnerException!.Message.Should().Be("Erro no AutoMapper");
        }


        #endregion

        #region Consulta do pedido

        [Theory]
        [InlineData(PedidoStatus.Processado)]
        [InlineData(PedidoStatus.Criado)]
        public async Task ListarPedidosPorStatusAsync_DeveRetornarPedidosFiltradosComSucesso(PedidoStatus status)
        {
            var serviceProvider = ServiceTestFactory.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();
            var repository = scope.ServiceProvider.GetRequiredService<IPedidoRepository>();

            var pedidosMock = new List<PedidoEntity>
            {
                new()
                {
                   Id = 1,
                   PedidoId = 101,
                   ClienteId = 1001,
                   Status = status,
                   Imposto = 15,
                   Itens = new List<PedidoItemEntity>{new() { ProdutoId = 500, Quantidade = 1, Valor = 25 }}
                },
                new()
                {
                   Id = 2,
                   PedidoId = 102,
                   ClienteId = 1002,
                   Status = status,
                   Imposto = 30,
                   Itens = new List<PedidoItemEntity>{new() { ProdutoId = 501, Quantidade = 2, Valor = 30 }}
                }
            };

            repository.ObterPorStatusAsync(status).Returns(pedidosMock);

            var resultado = await service.ListarPedidosPorStatusAsync(status);

            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);
            resultado.All(p => p.Status == status.ToString()).Should().BeTrue();
        }

        [Fact]
        public async Task ConsultarPedidoPorIdAsync_DeveRetornarPedidoQuandoExistirComSucesso()
        {
            var serviceProvider = ServiceTestFactory.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();
            var repository = scope.ServiceProvider.GetRequiredService<IPedidoRepository>();

            var pedido = new PedidoEntity
            {
                Id = 1,
                PedidoId = 101,
                ClienteId = 1001,
                Status = PedidoStatus.Criado,
                Imposto = 15,
                Itens = new List<PedidoItemEntity> { new() { ProdutoId = 500, Quantidade = 1, Valor = 25 } }
            };

            repository.ObterPorIdAsync(1).Returns(pedido);

            var resultado = await service.ConsultarPedidoPorIdAsync(1);

            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(pedido.Id);
            resultado.PedidoId.Should().Be(pedido.PedidoId);
            resultado.ClienteId.Should().Be(pedido.ClienteId);
            resultado.Imposto.Should().Be(pedido.Imposto);
            resultado.Status.Should().Be(pedido.Status.ToString());
            resultado.Itens.Should().HaveCount(1);
        }

        [Fact]
        public async Task ConsultarPedidoPorIdAsync_ErroAoEncontrarPedido_DeveLancarExcecao()
        {
            var serviceProvider = ServiceTestFactory.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();
            var repository = scope.ServiceProvider.GetRequiredService<IPedidoRepository>();

            repository.ObterPorIdAsync(1).Returns((PedidoEntity?)null);

            Func<Task> act = async () => await service.ConsultarPedidoPorIdAsync(1);

            var exception = await act.Should().ThrowAsync<ApplicationException>();

            exception.Which.Message.Should().Be("Erro ao consultar o pedido 1.");
            exception.Which.InnerException.Should().BeOfType<ApplicationException>();
            exception.Which.InnerException!.Message.Should().Be("Pedido não encontrado com o ID: 1");
        }

        [Fact]
        public async Task ListarPedidosPorStatusAsync_ErroNoRepositorio_DeveLancarExcecao()
        {
            var repository = Substitute.For<IPedidoRepository>();
            repository.ObterPorStatusAsync(Arg.Any<PedidoStatus>())
                      .Throws(new Exception("Falha ao acessar o repositório"));

            var serviceProvider = ServiceTestFactory.BuildServiceProvider(pedidoRepositoryMock: repository);
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();

            Func<Task> act = async () => await service.ListarPedidosPorStatusAsync(PedidoStatus.Criado);

            var exception = await act.Should().ThrowAsync<ApplicationException>();
            exception.Which.InnerException!.Message.Should().Be("Falha ao acessar o repositório");
        }

        [Fact]
        public async Task ConsultarPedidoPorIdAsync_PedidoNaoEncontrado_DeveLancarExcecao()
        {
            var repository = Substitute.For<IPedidoRepository>();
            repository.ObterPorIdAsync(Arg.Any<int>()).Returns((PedidoEntity?)null);

            var serviceProvider = ServiceTestFactory.BuildServiceProvider(pedidoRepositoryMock: repository);
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();

            Func<Task> act = async () => await service.ConsultarPedidoPorIdAsync(999);

            var exception = await act.Should().ThrowAsync<ApplicationException>();
            exception.Which.InnerException!.Message.Should().Be("Pedido não encontrado com o ID: 999");
        }

        #endregion
    }

}
