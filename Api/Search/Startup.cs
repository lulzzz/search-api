using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Search.Swagger;
using Search.Abstractions;
using Search.PostgresRDKit;

namespace Search
{
    public class Startup
    {
        const string connectionString = "User ID=postgres;Host=192.168.99.100;Port=5432;Database=simsearch;Pooling=true;";
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISearchProvider>(new PostgresRDKitSearchProvider(connectionString));

            services.AddCors();

            services
                .AddMvcCore()
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
                    c.OperationFilter<AddRequired>();
                    c.SwaggerDoc("v1", new Info { Title = "ChemSearch API", Version = "v1" });
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
                .UseMvc()
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChemSearch API V1");
                });
        }
    }
}
