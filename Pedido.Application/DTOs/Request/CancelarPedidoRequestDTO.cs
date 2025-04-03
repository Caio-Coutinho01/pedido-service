using Pedido.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Pedido.Application.DTOs.Request
{
    public class CancelarPedidoRequestDTO
    {
        [Required(ErrorMessage = "Justifique motivo do cancelamento!")]
        public string JustificativaCancelamento { get; set; }
    }
}
