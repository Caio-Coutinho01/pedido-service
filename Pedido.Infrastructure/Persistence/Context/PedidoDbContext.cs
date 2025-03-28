using Microsoft.EntityFrameworkCore;
using Pedido.Domain.Entities;

namespace Pedido.Infrastructure.Persistence.Context
{
    public class PedidoDbContext : DbContext
    {
        public PedidoDbContext(DbContextOptions<PedidoDbContext> options) : base(options)
        { }

        public DbSet<PedidoEntity> Pedidos { get; set; }
        public DbSet<PedidoItemEntity> Itens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PedidoEntity>()
                .HasMany(p => p.Itens)
                .WithOne(i => i.Pedido)
                .HasForeignKey(i => i.PedidoEntityId);

            modelBuilder.Entity<PedidoEntity>()
                .Property(p => p.Imposto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PedidoItemEntity>()
                .Property(i => i.Valor)
                .HasPrecision(18, 2);
        }
    }
}
