using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace _MediaInfoService.HealthCheck
{
    public class BaseHealthCheck: IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var healthCheckResultHealthy = true;

            if (healthCheckResultHealthy)
            {
                return
                    Task.FromResult(HealthCheckResult.Healthy("A healthy result."));
            }

            return 
                Task.FromResult(HealthCheckResult.Unhealthy("API is not running"));
        }
    }
}
