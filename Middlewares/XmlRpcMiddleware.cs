using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace com.b_velop.XmlRpc.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class XmlRpcMiddleware
    {
        private readonly RequestDelegate _next;
        protected ILogger<XmlRpcMiddleware> Logger { get; }
        private const string _response = @"<?xml version=""1.0""?><methodResponse><params><param><value><string>OK</string></value></param></params></methodResponse>";
        private IMemoryCache _cache;

        public XmlRpcMiddleware(
            IMemoryCache cache,
            ILogger<XmlRpcMiddleware> logger,
            RequestDelegate next)
        {
            Logger = logger;
            _cache = cache;
            _next = next;
        }

        public async Task Invoke(
            HttpContext httpContext)
        {
            if (httpContext.Request.Method == "POST" && httpContext.Request.Path == "/RPC2")
            {
                Parser.Parse(Logger, httpContext.Request.Body);

                httpContext.Response.Headers.Add("Content-Type", "text/xml; charset=iso-8859-1");
                httpContext.Response.Headers.Add("Content-Length", _response.Length.ToString());
                httpContext.Response.StatusCode = 200;

                using (var sw = new StreamWriter(httpContext.Response.Body))
                    await sw.WriteAsync(_response);
                _cache.Set(Strings.LastConnection, DateTime.Now);
            }
            return;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class XmlRpcMiddlewareExtensions
    {
        public static IApplicationBuilder UseXmlRpc(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<XmlRpcMiddleware>();
        }
    }


    public class Parser
    {
        public static HomematicValueList HomematicValues = new HomematicValueList();
        public static string[] IDs = new string[0];
        public static void Parse(
            ILogger<XmlRpcMiddleware> logger,
            Stream inputString)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(inputString);

                var nodeList = doc.GetElementsByTagName("value");

                if (nodeList == null)
                    return;

                for (var i = 0; i < nodeList.Count; i++)
                {
                    var n = nodeList[i]; // as XmlNode;
                    if (n?.InnerXml == Strings.InstanceId)
                    {
                        var hv = new HomematicValue
                        {
                            Time = DateTimeOffset.Now,
                            Instance = n.InnerXml,
                            Id = nodeList[++i].InnerXml,
                            Name = nodeList[++i].InnerXml,
                            Value = nodeList[++i].InnerText,
                            Type = nodeList[i].FirstChild.Name,
                        };
                        logger.LogInformation(hv.Id);
                        if (IDs.Contains(hv.AllId))
                        {
                            HomematicValues.Add(hv);
                            logger.LogInformation($"{hv} added to list");
                        }
                    }
                }
                logger.LogInformation($"{HomematicValues.Count} items in list.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while parsing");
            }
        }
    }
}
