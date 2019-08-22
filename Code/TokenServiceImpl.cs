using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace com.b_velop.XmlRpc.Code
{
    public class TokenServiceImpl : TokenService
    {
        protected ILogger<TokenService> Logger;
        private IMemoryCache _cache;
        private HttpClient _client;
        private IServiceProvider _services;

        public TokenServiceImpl(
            HttpClient client,
            IServiceProvider services,
            IMemoryCache cache,
            ILogger<TokenService> logger)
        {
            _services = services;
            _client = client;
            var secrets = services.GetRequiredService<Secrets>();
            _client.BaseAddress = new Uri(secrets.Issuer);
            _cache = cache;
            Logger = logger;
        }

        public async Task<Token> RequestTokenAsync()
        {

            if (_cache.TryGetValue(Strings.Token, out Token token) && _cache.TryGetValue(Strings.Expiration, out DateTime valid))
            {
                if (valid > DateTime.Now)
                    return token;
            }

            var secrets = _services.GetRequiredService<Secrets>();

            var credentials = Encoding.ASCII.GetBytes($"{secrets.ClientId}:{secrets.Secret}");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", GetBase64Credentials(credentials));
            var dict = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials"},
                { "scope", secrets.Scope }
            };

            var content = new FormUrlEncodedContent(dict);
            var response = await _client.PostAsync("/connect/token", content);
            var ts = await response.Content.ReadAsStringAsync();
            token = JsonConvert.DeserializeObject<Token>(ts);

            _cache.Set(Strings.Token, token);
            _cache.Set(Strings.Expiration, DateTime.Now.AddSeconds(token.ExpiresIn));

            return token;
        }
        public string GetBase64Credentials(byte[] credentials)
            => Convert.ToBase64String(credentials);
    }
}
