using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sala7ly.BLL.Services.Abstraction;
using Sala7ly.DAL.Repositories.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class TechnicianEmbeddingSyncService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<TechnicianEmbeddingSyncService> _logger;

        private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(1);
        private const int StaleAfterDays = 7;

        public TechnicianEmbeddingSyncService(
            IServiceProvider services,
            ILogger<TechnicianEmbeddingSyncService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "TechnicianEmbeddingSyncService started. Interval: {Interval}, StaleAfter: {Days}d.",
                SyncInterval, StaleAfterDays);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunSyncPassAsync(stoppingToken);
                await Task.Delay(SyncInterval, stoppingToken);
            }
        }

        private async Task RunSyncPassAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _services.CreateScope();
                var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
                var technicianRepo = scope.ServiceProvider.GetRequiredService<ITechnicianProfileRepository>();

                var stale = await technicianRepo.GetWithOutdatedEmbeddingsAsync(StaleAfterDays);

                if (!stale.Any())
                {
                    _logger.LogDebug("Embedding sync: all vectors up to date.");
                    return;
                }

                _logger.LogInformation("Embedding sync: {Count} technician(s) need (re)indexing.", stale.Count);

                int succeeded = 0, failed = 0;

                foreach (var technician in stale)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var text = embeddingService.BuildTechnicianText(technician);
                        technician.EmbeddingVector = await embeddingService.GetEmbeddingAsync(text);
                        technician.EmbeddingUpdatedAt = DateTime.UtcNow;
                        technicianRepo.Update(technician);
                        succeeded++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.LogWarning(ex, "Embedding sync: failed for technician {Id}.", technician.Id);
                    }
                }

                if (succeeded > 0)
                    await technicianRepo.SaveChangesAsync();

                _logger.LogInformation("Embedding sync complete: {Ok} ok, {Fail} failed.", succeeded, failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Embedding sync pass fatal error.");
            }
        }
    }
}