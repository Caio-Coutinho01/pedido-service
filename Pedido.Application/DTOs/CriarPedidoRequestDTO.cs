namespace Pedido.Application.DTOs
{
    public class CriarPedidoRequestDTO
    {
        public int PedidoId { get; set; }
        public int ClienteId { get; set; }
        public List<ItemPedidoDTO> Itens { get; set; }
    }

    public class ItemPedidoDTO
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }
    }
}
