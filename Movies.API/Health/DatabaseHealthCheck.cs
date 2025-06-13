using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.API.Health
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public const string Name = "Database";

        public DatabaseHealthCheck(IDbConnectionFactory dbConnectionFactory, ILogger<DatabaseHealthCheck> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
        {
            try
            {
                _ = _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
                return HealthCheckResult.Healthy("Database is up and running.");
            }
            catch (Exception ex)
            {
                const string errorMessage = "Database connection failed. Please check the database server and connection settings.";
                _logger.LogError(errorMessage, ex);
                return HealthCheckResult.Unhealthy(errorMessage, ex);

            }
        }
    }
}
