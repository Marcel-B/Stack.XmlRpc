using System.Net.Mime;
using com.b_velop.XmlRpc.Code;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Hosted;
using com.b_velop.XmlRpc.Services.Http;
using GraphQL.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XmlRpc.Middlewares;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace XmlRpc
{
    public class Startup
    {
        private IHostingEnvironment Environment { get; }

        public Startup(
            IHostingEnvironment environment)
        {
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var clientId = System.Environment.GetEnvironmentVariable("ClientId");
            var scope = System.Environment.GetEnvironmentVariable("Scope");
            var secret = System.Environment.GetEnvironmentVariable("Secret");
            var issuer = System.Environment.GetEnvironmentVariable("Issuer");
            var homematicEndpoint = System.Environment.GetEnvironmentVariable("HomematicEndpoint");
            var instanceName = System.Environment.GetEnvironmentVariable("InstanceName");
            var instanceEndpoint = System.Environment.GetEnvironmentVariable("InstanceEndpoint");

            Strings.InstanceId = instanceName;
            Strings.MyUrl = instanceEndpoint;

            services.AddMemoryCache();
            services.AddHostedService<CcuConnectionWatcher>();

            services.AddHttpClient<CcuConnector, CcuConnectorImpl>();

            services.AddHttpClient<TokenService, TokenServiceImpl>();

            services.AddScoped<ActiveMeasurePointService, ActiveMeasurePointServiceImpl>();
            services.AddScoped<GraphQLClient>(x => new GraphQLClient("https://data.qaybe.de/graphql"));



            services.AddSingleton(new Secrets
            {
                ClientId = clientId,
                Scope = scope,
                Secret = secret,
                Issuer = issuer,
                HomematicEndpoint = homematicEndpoint,
                InstanceEndpoint = instanceEndpoint,
                InstanceName = instanceName
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseXmlRpc();
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
