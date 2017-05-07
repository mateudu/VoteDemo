using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Swagger;
using Vote.Domain.Application.Commands;
using Vote.Domain.Application.Events;
using Vote.Domain.Application.Handlers;
using Vote.Domain.Models;
using VoteDemo.BuildingBlocks.EventBus;
using VoteDemo.BuildingBlocks.EventBus.Abstractions;
using VoteDemo.BuildingBlocks.RabbitMQEventBus;

namespace Vote.API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.Configure<VoteAPISettings>(Configuration);

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<VoteAPISettings>>().Value;

                //because of https://github.com/dotnet/corefx/issues/8768
                var dnsTask = Dns.GetHostAddressesAsync(settings.RedisHost);
                var addresses = dnsTask.Result.Where(x=>x.AddressFamily==AddressFamily.InterNetwork).ToList();
                
                settings.RedisPort = int.TryParse(settings.RedisPort, out var s) ? settings.RedisPort : "6379";
                string connString = $"{addresses[0].MapToIPv4().ToString()}:{settings.RedisPort},ssl=False,abortConnect=False";
                return ConnectionMultiplexer.Connect(connString);
            });

            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<VoteAPISettings>>().Value;
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var factory = new ConnectionFactory()
                {
                    HostName = settings.EventBusConnection
                };
                return new DefaultRabbitMQPersistentConnection(factory, logger);
            });

            services.AddSwaggerGen(c =>
            {
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info()
                {
                    Title = "Vote HTTP API",
                    Version = "v1",
                    Description = "The Vote Service HTTP API",
                    TermsOfService = "Terms Of Service"
                });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddTransient<IVoteRepository, RedisVoteRepository>();
            services.AddSingleton<IEventBus, RabbitMQEventBus>();
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            services.AddTransient<ElectionMadeEventHandler>();
            services.AddTransient<VoteMadeEventHandler>();
            services.AddTransient<VoteOptionAddedEventHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vote API V1");
            });


            var electionMadeHandler = app.ApplicationServices
                .GetService<IIntegrationEventHandler<ElectionMadeEvent>>();

            var voteMadeHandler = app.ApplicationServices
                .GetService<IIntegrationEventHandler<VoteMadeEvent>>();

            var voteOptionAddedHandler = app.ApplicationServices
                .GetService<IIntegrationEventHandler<VoteOptionAddedEvent>>();

            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<ElectionMadeEvent, ElectionMadeEventHandler>
                (() => app.ApplicationServices.GetRequiredService<ElectionMadeEventHandler>());

            eventBus.Subscribe<VoteMadeEvent, VoteMadeEventHandler>
                (() => app.ApplicationServices.GetRequiredService<VoteMadeEventHandler>());

            eventBus.Subscribe<VoteOptionAddedEvent, VoteOptionAddedEventHandler>
                (() => app.ApplicationServices.GetRequiredService<VoteOptionAddedEventHandler>());
        }
    }
}
