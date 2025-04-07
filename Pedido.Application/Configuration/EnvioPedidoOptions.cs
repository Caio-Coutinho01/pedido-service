using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pedido.Application.Configuration
{
    public class EnvioPedidosOptions
    {
        public int MaxTentativas { get; set; } = 3;
    }
}
