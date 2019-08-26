using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Services.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            _cache.Set(Strings.AlarmActive, false);
            _logger = logger;
            _services = services;
            _cache.Set(Strings.AlarmIds, new Dictionary<string, bool>
            {
                // Alarmknopf
                {"NEQ0889879:1:PRESS_SHORT", false}, // AUS
                {"NEQ0889879:1:PRESS_LONG", false}, // AUS
                {"NEQ0889879:2:PRESS_SHORT", true}, // AN
                {"NEQ0889879:2:PRESS_LONG", true}, // AN

            });
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
            using (var sco = _services.CreateScope())
            {
                var alarm = sco.ServiceProvider.GetRequiredService<AlarmService>();
                await alarm.UpdateAlarmAsync();
            }

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
                DateTime.Now - lastUpload > TimeSpan.FromMinutes(10))
            {
                if (_cache.TryGetValue(Strings.Values, out HomematicValueList values))
                {
                    using (var scope = _services.CreateScope())
                    {
                        var uploadService = scope.ServiceProvider.GetRequiredService<DataUploadService>();
                        await uploadService.UploadValuesAsync();
                    }
                    _cache.Set(Strings.LastUpload, DateTime.Now);
                }
            }

            if (!_cache.TryGetValue(Strings.LastActiveMeasurePointsPull, out DateTime lastActivePull) ||
                DateTime.Now - lastActivePull > TimeSpan.FromHours(1))
            {
                using (var scope = _services.CreateScope())
                {
                    var activeMeasurePointService = scope.ServiceProvider.GetRequiredService<ActiveMeasurePointService>();
                    var activeMeasurePoints = await activeMeasurePointService.GetActiveMeasurePointsAsync();

                    if (activeMeasurePoints == null)
                        return;

                    var activeIds = activeMeasurePoints.Where(_ => _.IsActive).Select(_ => _.ExternId);
                    _cache.Set(Strings.ActiveMeasurePoints, activeIds.ToArray());
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
