using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Services.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Code
{
    public class CcuConnectorImpl : CcuConnector
    {
        private ILogger<CcuConnectorImpl> _logger;
        private IMemoryCache _cache;
        private HttpClient _client;

        public CcuConnectorImpl(
            HttpClient client,
            IMemoryCache cache,
            ILogger<CcuConnectorImpl> logger)
        {
            _client = client;
            _client.Timeout = TimeSpan.FromSeconds(5);
            _cache = cache;
            _logger = logger;
            _client.BaseAddress = new Uri("http://homematic-raspi:2001");
        }

        public async Task ConnectToCcuAsync()
        {
            _client.DefaultRequestHeaders.Clear();
            //_client.DefaultRequestHeaders.Add("Content-Length", Strings.Init.Length.ToString());
            try
            {
                _logger.LogInformation(2225, $"Connect:\n{Strings.Init}");
                var content = new StringContent(Strings.Init, Encoding.Default, "text/xml");
                var response = await _client.PostAsync("/", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(result);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(2225, e, $"Error occurred while connecting to ccu '{_client.BaseAddress}'");
            }
        }

        public async Task DisconnectCcu()
        {
            _client.DefaultRequestHeaders.Clear();
            //_client.DefaultRequestHeaders.Add("Content-Length", Strings.Init.Length.ToString());
            try
            {
                _logger.LogInformation($"Disconnect:\n{Strings.Init}");
                var content = new StringContent(Strings.Deinit, Encoding.Default, "text/xml");
                var response = await _client.PostAsync("/", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(result);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error");
            }
        }
    }
}
