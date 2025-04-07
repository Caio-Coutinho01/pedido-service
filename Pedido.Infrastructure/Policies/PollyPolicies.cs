using Microsoft.Extensions.Logging;
using Polly;

namespace Pedido.Infrastructure.Policies;
public static class PollyPolicies
{
    public static IAsyncPolicy WrapResilientPolicy(ILogger logger)
    {
        var retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (exception, timeSpan, retryCount, _) =>
                {
                    logger.LogWarning(exception, $"Tentativa {retryCount} falhou. Retentando em {timeSpan.TotalSeconds}s...");
                });

        var breaker = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(15, TimeSpan.FromSeconds(30),
                onBreak: (ex, ts) => logger.LogWarning($"Circuit breaker ativado por {ts.TotalSeconds}s. Erro: {ex.Message}"),
                onReset: () => logger.LogInformation("Circuit breaker resetado."));

        return Policy.WrapAsync(retry, breaker);
    }
}
