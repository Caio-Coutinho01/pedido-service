using Microsoft.EntityFrameworkCore;
using Pedido.Infrastructure.Persistence.Context;
using Testcontainers.MsSql;

namespace Pedido.Tests.Builders.Base
{
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly MsSqlContainer _dbContainer;
        protected PedidoDbContext DbContext;

        public IntegrationTestBase()
        {
            _dbContainer = new MsSqlBuilder().WithPassword("yourStrong(!)Password").WithName("pedido_test_db").Build();
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            var options = new DbContextOptionsBuilder<PedidoDbContext>().UseSqlServer(_dbContainer.GetConnectionString()).Options;

            DbContext = new PedidoDbContext(options);
            await DbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }
    }
}
