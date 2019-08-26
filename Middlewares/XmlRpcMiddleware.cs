using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Middlewares
{
    public class XmlRpcMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<XmlRpcMiddleware> _logger;
        private const string Response = @"<?xml version=""1.0""?><methodResponse><params><param><value><string>OK</string></value></param></params></methodResponse>";
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _services;

        public XmlRpcMiddleware(
            IMemoryCache cache,
            IServiceProvider services,
            ILogger<XmlRpcMiddleware> logger,
            RequestDelegate next)
        {
            _cache = cache;
            _services = services;
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(
            HttpContext httpContext)
        {
            if (httpContext.Request.Method == "POST" && httpContext.Request.Path == "/RPC2")
            {
                _cache.TryGetValue(Strings.AlarmIds, out Dictionary<string, bool> alarmIds);

                using (var scope = _services.CreateScope())
                {
                    var homematicParser = scope.ServiceProvider.GetRequiredService<Parser>();
                    await homematicParser.Parse(httpContext.Request.Body);
                }

                httpContext.Response.Headers.Add("Content-Type", "text/xml; charset=iso-8859-1");
                httpContext.Response.Headers.Add("Content-Length", Response.Length.ToString());
                httpContext.Response.StatusCode = 200;

                using (var streamWriter = new StreamWriter(httpContext.Response.Body))
                    await streamWriter.WriteAsync(Response);

                _cache.Set(Strings.LastConnection, DateTime.Now);
            }
            return;
        }
    }

    public static class XmlRpcMiddlewareExtensions
    {
        public static IApplicationBuilder UseXmlRpc(
            this IApplicationBuilder builder)
            => builder.UseMiddleware<XmlRpcMiddleware>();
    }
}
