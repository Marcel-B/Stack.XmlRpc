using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using com.b_velop.XmlRpc.BL;
using com.b_velop.XmlRpc.Code;
using com.b_velop.XmlRpc.Constants;
using com.b_velop.XmlRpc.Contexts;
using com.b_velop.XmlRpc.Middlewares;
using com.b_velop.XmlRpc.Models;
using com.b_velop.XmlRpc.Services.Hosted;
using com.b_velop.XmlRpc.Services.Http;
using GraphQL.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace com.b_velop.XmlRpc
{
    public class Startup
    {
        private IHostingEnvironment Environment { get; }
        ILogger<Startup> Logger { get; }

        public Startup(
            ILogger<Startup> logger,
            IHostingEnvironment environment)
        {
            Logger = logger;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            Hashtable evs = (Hashtable)System.Environment.GetEnvironmentVariables();
            foreach (var e in evs)
            {
                var l = (DictionaryEntry)e;
                Logger.LogInformation($"Environment Variable: {l.Key} = {l.Value}");
            }

            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<HomeContext>();

            var clientId = System.Environment.GetEnvironmentVariable("ClientId");
            var scope = System.Environment.GetEnvironmentVariable("Scope");
            var secret = System.Environment.GetEnvironmentVariable("Secret");
            var issuer = System.Environment.GetEnvironmentVariable("Issuer");
            var homematicEndpoint = System.Environment.GetEnvironmentVariable("HomematicEndpoint");
            var instanceName = System.Environment.GetEnvironmentVariable("InstanceName");
            var instanceEndpoint = System.Environment.GetEnvironmentVariable("InstanceEndpoint");
            var alarmLiving = System.Environment.GetEnvironmentVariable("AlarmLiving");
            var alarmFloor = System.Environment.GetEnvironmentVariable("AlarmFloor");

            Strings.InstanceId = instanceName;
            Strings.MyUrl = instanceEndpoint;

            services.AddMemoryCache();
            services.AddHostedService<CcuConnectionWatcher>();

            services.AddHttpClient<CcuConnector, CcuConnectorImpl>();
            services.AddHttpClient<AlarmService, AlarmServiceImpl>();
            services.AddHttpClient<TokenService, TokenServiceImpl>();

            services.AddScoped<ActiveMeasurePointService, ActiveMeasurePointServiceImpl>();
            services.AddScoped<DataUploadService, DataUploadServiceImpl>();
            services.AddScoped<GraphQLClient>(x => new GraphQLClient("https://data.qaybe.de/graphql"));
            services.AddScoped<Parser, ParserImpl>();

            services.AddScoped(_ => new Secrets
            {
                ClientId = clientId,
                Scope = scope,
                Secret = secret,
                Issuer = issuer,
                HomematicEndpoint = homematicEndpoint,
                InstanceEndpoint = instanceEndpoint,
                InstanceName = instanceName,
                AlarmFloor = alarmFloor,
                AlarmLiving = alarmLiving
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
