using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Services.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XmlRpc.Middlewares;

namespace com.b_velop.XmlRpc.Services.Hosted
{
    public class CcuConnectionWatcher : IHostedService, IDisposable
    {
        private readonly ILogger<CcuConnectionWatcher> _logger;
        private Timer _timer;
        private readonly IMemoryCache _cache;
        private readonly CcuConnector _service;
        private readonly IServiceProvider _services;

        public CcuConnectionWatcher(
            IMemoryCache cache,
            IServiceProvider services,
            CcuConnector service,
            ILogger<CcuConnectionWatcher> logger)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
            _services = services;
        }

        public Task StartAsync(
            CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private async void DoWork(
            object state)
        {
            if (!_cache.TryGetValue(Strings.LastConnection, out DateTime time))
            {
                _logger.LogInformation("No Connection. Start connecting to CCU");
                await _service.ConnectToCcuAsync();
                _cache.Set(Strings.LastConnection, DateTime.Now);

            }
            else
            {
                if (DateTime.Now - time > TimeSpan.FromMinutes(5))
                {
                    _logger.LogInformation("Reconnect to CCU");
                    await _service.ConnectToCcuAsync();
                }
            }

            if (!_cache.TryGetValue(Strings.LastUpload, out DateTime lastUpload) ||
                DateTime.Now - lastUpload > TimeSpan.FromMinutes(5))
            {
                var values = Parser.HomematicValues.WithdrawItems();
                foreach (var homematicValue in values)
                {
                    _logger.LogInformation($"Item:\n{homematicValue}\n");
                }

                _cache.Set(Strings.LastUpload, DateTime.Now);
            }


            if (!_cache.TryGetValue(Strings.LastActiveMeasurePointsPull, out DateTime lastActivePull) ||
                DateTime.Now - lastActivePull > TimeSpan.FromHours(1))
            {
                using (var scope = _services.CreateScope())
                {
                    var activeMeasurePointService = scope.ServiceProvider.GetRequiredService<ActiveMeasurePointService>();
                    var activeMeasurePoints = await activeMeasurePointService.GetActiveMeasurePointsAsync();
                    _cache.Set(Strings.ActiveMeasurePoints, activeMeasurePoints.ToArray());
                }
                _cache.Set(Strings.LastActiveMeasurePointsPull, DateTime.Now);
            }
        }

        public Task StopAsync(
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
