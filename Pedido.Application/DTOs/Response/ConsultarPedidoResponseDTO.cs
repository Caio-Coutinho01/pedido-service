using Pedido.Application.DTOs.Request;
using System.Text.Json.Serialization;

namespace Pedido.Application.DTOs.Response
{
    public class ConsultarPedidoResponseDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ClienteId { get; set; }
        public decimal Imposto { get; set; }
        public string Status { get; set; }
        public List<ItemPedidoDTO> Itens { get; set; } = new();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JustificativaCancelamento { get; set; }
    }
}
