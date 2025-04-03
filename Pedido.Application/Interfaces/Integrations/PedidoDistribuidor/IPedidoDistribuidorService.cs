using Pedido.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pedido.Application.Interfaces.Integrations.PedidoDistribuidor
{
    public interface IPedidoDistribuidorService
    {
        Task DistribuirPedidoAsync(ConsultarPedidoResponseDTO pedido);
    }
}
