using System;
using System.Net.Http;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Contexts;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public class AlarmServiceImpl : AlarmService
    {
        private readonly HttpClient _client;
        private readonly HomeContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AlarmService> _logger;
        private readonly Secrets _secrets;

        public AlarmServiceImpl(
            HttpClient client,
            HomeContext homeContext,
            Secrets secrets,
            IMemoryCache cache,
            ILogger<AlarmService> logger)
        {
            _client = client;
            _db = homeContext;
            _secrets = secrets;
            _cache = cache;
            _logger = logger;
            _client.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task UpdateAlarmAsync()
        {
            try
            {
                if (!_cache.TryGetValue(Strings.AlarmActive, out bool isActive))
                {
                    isActive = _db.States.Find(1).State;
                    _cache.Set(Strings.AlarmActive, isActive);
                }
                else
                {
                    var state = isActive ? "ON" : "OFF";
                    var parameter = $"/?ALARM={state}";
                    if (!string.IsNullOrWhiteSpace(_secrets.AlarmFloor))
                    {
                        try
                        {
                            var response = await _client.GetAsync($"{_secrets.AlarmFloor}{parameter}");
                            if (!response.IsSuccessStatusCode)
                                _logger.LogWarning($"Error while calling alarm {_client.BaseAddress}{parameter}: ({(int)response.StatusCode}){response.ReasonPhrase}");
                        }
                        catch (OperationCanceledException e)
                        {
                            _logger.LogError(2220, e, $"Error occurred while updating '{_secrets.AlarmFloor}' alarmFloor");
                        }
                    }
                    else
                    {
                        _logger.LogWarning(2220, $"No uri for AlarmFloor");
                    }
                    if (!string.IsNullOrWhiteSpace(_secrets.AlarmLiving))
                    {
                        try
                        {
                            var response = await _client.GetAsync($"{_secrets.AlarmLiving}{parameter}");
                            if (!response.IsSuccessStatusCode)
                                _logger.LogWarning($"Error while calling alarm {_client.BaseAddress}{parameter}: ({(int)response.StatusCode}){response.ReasonPhrase}");
                        }
                        catch (OperationCanceledException e)
                        {
                            _logger.LogError(2220, e, $"Error occurred while updating '{_secrets.AlarmLiving}' alarmLiving");
                        }
                    }
                    else
                    {
                        _logger.LogWarning(2220, $"No uri for AlarmLiving");
                    }
                    return;
                }
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(2220, e, $"Unexpected error in UpdateAarmService");
            }
            return;
        }
    }
}
