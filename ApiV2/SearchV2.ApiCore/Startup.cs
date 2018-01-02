using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;
using Serilog;
using Serilog.Configuration;
using Serilog.Context;
using Microsoft.AspNetCore.Http.Extensions;
using System;

namespace SearchV2.ApiCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services
                .AddMvcCore()
                .AddSearchFeature()
                .AddApiExplorer()
                .AddDataAnnotations()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddJsonFormatters();


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new Info { Title = "ChemSearch API", Version = "v2" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var logPath = env.IsDevelopment()
                ? "/searchV2_logs/"
                : "../log/";

            var log = Log.Logger = new LoggerConfiguration().WriteTo.File(logPath, rollingInterval: RollingInterval.Month, retainedFileCountLimit: 2).CreateLogger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .Use(async (context, next) => {
                    var r = context.Request;
                    using (LogContext.PushProperty("HttpRequestMethod", r.Method))
                    using (LogContext.PushProperty("HttpRequestUri", r.GetDisplayUrl()))
                    {
                        try
                        {
                            await next.Invoke();
                        }
                        catch (Exception e)
                        {
                            log.Error(e, "Uncaught exception");
                            throw;
                        }
                    }
                })
                .UseMvc()
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "ChemSearch API V2");
                });
        }
    }
}
