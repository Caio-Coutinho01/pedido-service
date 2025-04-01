using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using NSubstitute;
using Pedido.Application.DTOs;
using Pedido.Application.Mappings;
using Pedido.Application.Services;
using Pedido.Infrastructure.Persistence.Context;
using TestInfrastructure;

namespace Pedido.Tests.Application.Services
{
    public class PedidoServiceTests
    {
        private readonly IFeatureManager _featureManager = Substitute.For<IFeatureManager>();
        private readonly ILogger<PedidoService> _logger = Substitute.For<ILogger<PedidoService>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly DbContextOptions<PedidoDbContext> _dbContextOptions;
        private readonly ServiceProvider _serviceProvider;

        public PedidoServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PedidoProfile>();
            });
            _mapper = config.CreateMapper();

            _dbContextOptions = new DbContextOptionsBuilder<PedidoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CriarPedidoAsync_UtilizandoOsDoisCalculos_DeveCriarPedidoComSucesso(bool usarNovaRegraImposto)
        {
            var serviceProvider = ServiceTestFactory.BuildServiceProvider(_dbContextOptions, usarNovaRegraImposto);

            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PedidoService>();

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 1,
                ClienteId = 10,
                Itens = new List<ItemPedidoDTO>{ new(){ ProdutoId = 100, Quantidade = 2, Valor = 50 }}
            };

            var response = await service.CriarPedidoAsync(request);

            response.Should().NotBeNull();
            response.Id.Should().BeGreaterThan(0);
            response.Status.Should().Be("Criado");

            var context = scope.ServiceProvider.GetRequiredService<PedidoDbContext>();
            var pedidoCriado = await context.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync();

            if (usarNovaRegraImposto)
                pedidoCriado!.Imposto.Should().Be(20);
            else
                pedidoCriado!.Imposto.Should().Be(30);
        }

    }

}
