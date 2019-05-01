using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Swashbuckle.AspNetCore.Swagger;
using CorrelationId;
using Halcyon.Web.HAL.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using SurveyWebApplication.Helpers;

namespace SurveyWebApplication
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private const string SwaggerDocumentName = "v1";
        private string _applicationVersion;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = CreateLoggingConfiguration();
        }

        /// <summary>
        /// Sets all possible input file for the configuration.
        /// The order how the files are added in the configuration is important,
        /// as the last added and found file's configuration is used finally.
        /// </summary>
        /// <returns>The configuration.</returns>
        private static IConfiguration SetEnvironmentConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private Serilog.ILogger CreateLoggingConfiguration()
        {
            _applicationVersion = Configuration["ApplicationVersion"];

            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("providerName", typeof(Program).Assembly.GetName().Name)
                .Enrich.WithProperty("applicationVersion", _applicationVersion)
                .WriteTo.Console(new ElasticsearchJsonFormatter())
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcCoreBuilder = services.AddMvcCore();
            mvcCoreBuilder.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            mvcCoreBuilder.AddJsonFormatters();

            ConfigureHalJsonAsDefault(mvcCoreBuilder);
            ConfigureSwaggerService(services, mvcCoreBuilder);
            services.AddCorrelationId();
            services.AddTransient<IJsonFileReaderWriter, JsonFileReaderWriter>();
        }

        private void ConfigureHalJsonAsDefault(IMvcCoreBuilder mvcCoreBuilder)
        {
            mvcCoreBuilder.AddMvcOptions(config =>
            {
                config.OutputFormatters.RemoveType<JsonOutputFormatter>();
                config.OutputFormatters.Add(new JsonHalOutputFormatter(
                    new[]
                    {
                        "application/hal+json"
                    }));
            });
        }

        private void ConfigureSwaggerService(IServiceCollection services, IMvcCoreBuilder mvcCoreBuilder)
        {
            // Swashbuckle relies heavily on ApiExplorer so we add it here
            mvcCoreBuilder.AddApiExplorer();

            // Register the Swagger generator
            services.AddSwaggerGen(genOptions =>
            {
                // Defining one Swagger document for our service
                genOptions.SwaggerDoc(SwaggerDocumentName, new Info
                {
                    Version = _applicationVersion,
                    Title = "Survey",
                    Description = "Here you can find the All the APIs related to Candidate Recruitment Survey Microservice",
                    Contact = new Contact
                    {
                        Name = "usery9ed5jvb"
                    }
                });

                genOptions.EnableAnnotations();

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            // Ensure any buffered events are sent at shutdown
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCorrelationId(new CorrelationIdOptions() { UseGuidForCorrelationId = true, Header = "Request-Id" });
            EnableSwaggerMiddleware(app);
            app.UseMvc();
        }

        private static void EnableSwaggerMiddleware(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.) specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{SwaggerDocumentName}/swagger.json", "Survey APIs");
                c.RoutePrefix = "swagger";
            });
        }
    }
}
