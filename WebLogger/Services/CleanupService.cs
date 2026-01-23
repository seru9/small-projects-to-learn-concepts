using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using PcsSelcomWebLogger.Data;
namespace PcsSelcomWebLogger.Services 
{
    public class CleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupService> _logger;


        public CleanupService(IServiceProvider serviceProvider, ILogger<CleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DeleteOldLogsAsync();
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        private async Task DeleteOldLogsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppLogsContext>();
                var cutoff = DateTime.Now.AddDays(-7);
                var oldLogs = db.AppLogs.Where(log => log.DateTime < cutoff);
                int count = oldLogs.Count();

                if (count > 0)
                {
                    db.AppLogs.RemoveRange(oldLogs);
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"Deleted {count} old log(s).");
                }
                else
                {
                    _logger.LogInformation("No old logs to delete.");
                }
            }
        }
    }
}
