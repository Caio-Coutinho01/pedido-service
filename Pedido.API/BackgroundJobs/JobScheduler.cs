using Hangfire;
using Pedido.Application.Interfaces;

namespace Pedido.API.BackgroundJobs;

public static class JobScheduler
{
    public static void ConfigurarJobs()
    {
        RecurringJob.AddOrUpdate<IPedidoService>("enviar-pedidos-elegiveis", service => service.EnviarPedidosCriadosAsync(), "*/5 * * * *");
    }
}
