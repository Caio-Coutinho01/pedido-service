namespace Pedido.Domain.Entities
{
    public class PedidoItemEntity
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }

        public int PedidoEntityId { get; set; }
        public PedidoEntity Pedido { get; set; }
    }
}
