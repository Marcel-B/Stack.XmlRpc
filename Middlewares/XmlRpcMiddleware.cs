using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace XmlRpc.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class XmlRpcMiddleware
    {
        private readonly RequestDelegate _next;
        protected ILogger<XmlRpcMiddleware> Logger { get; }
        private const string _response = @"<?xml version=""1.0""?><methodResponse><params><param><value><string>OK</string></value></param></params></methodResponse>";

        public XmlRpcMiddleware(
            ILogger<XmlRpcMiddleware> logger,
            RequestDelegate next)
        {
            Logger = logger;
            _next = next;
        }

        public async Task Invoke(
            HttpContext httpContext)
        {
            if (httpContext.Request.Method == "POST")
            {
                //using (var sr = new StreamReader(httpContext.Request.Body))
                //{
                //var body = await sr.ReadToEndAsync();
                //Logger.LogInformation(body);
                Parser.Parse(Logger, httpContext.Request.Body);
                //}

                httpContext.Response.Headers.Add("Content-Type", "text/xml; charset=iso-8859-1");
                httpContext.Response.Headers.Add("Content-Length", _response.Length.ToString());
                httpContext.Response.StatusCode = 200;

                using (var sw = new StreamWriter(httpContext.Response.Body))
                    await sw.WriteAsync(_response);
            }
            return;// _next(httpContext);
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
        public class Saver
        {
            public DateTimeOffset Time { get; set; }
            public string Instance { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
            public override string ToString()
            {
                return $"{Time}\n  {Id}\n   {Name}\n   {Value} ({Type})";
            }
        }

        public static void Parse(
            ILogger<XmlRpcMiddleware> logger,
            Stream inputString)
        {
            var doc = new XmlDocument();
            doc.Load(inputString);

            XmlNodeList nodeList = doc.GetElementsByTagName("value");
            if (nodeList != null)
                for (var i = 0; i < nodeList.Count; i++)
                {
                    var n = nodeList[i];// as XmlNode;
                    if (n?.InnerXml == "Laptop")
                    {
                        var r = new Saver
                        {
                            Time = DateTimeOffset.Now,
                            Instance = n.InnerXml,
                            Id = nodeList[++i].InnerXml,
                            Name = nodeList[++i].InnerXml,
                            Value = nodeList[++i].InnerText,
                            Type = nodeList[i].FirstChild.Name,
                        };
                        logger.LogInformation(r.ToString());
                    }
                }
        }
    }
}
