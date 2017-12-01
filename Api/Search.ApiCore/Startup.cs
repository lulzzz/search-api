using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection.Emit;
using System.Linq;
using Search.Abstractions;

namespace Search.ApiCore
{
    public class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var catalogServiceTypeArguments = services
                .FirstOrDefault(s => s.ServiceType.IsConstructedGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(ICatalog<,>))
                ?.ServiceType.GenericTypeArguments;

            services.AddCors();
            
            services
                .AddMvcCore()
                .AddApplicationPart(typeof(Startup).Assembly)
                .ConfigureApplicationPartManager(apm =>
                {
                    apm.FeatureProviders.Add(new MoleculesControllerFeatureProvider(catalogServiceTypeArguments[0], catalogServiceTypeArguments[1]));
                })
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
