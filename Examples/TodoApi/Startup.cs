using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using TodoApi.Features.Shared;

namespace TodoApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCosmosDatabase<TodoDatabase>(_configuration.GetConnectionString("CosmosDb"))
                .AddSwaggerGen(config =>
                {
                    config.SwaggerDoc("v1", new Info { Title = "TodoApi", Version = "v1" });
                    config.DescribeAllParametersInCamelCase();
                    config.DescribeAllEnumsAsStrings();
                    config.DescribeStringEnumsInCamelCase();
                })
                .AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app
                .UseSwagger()
                .UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi v1"))
                .UseMvc()
                .Run((context) =>
                {
                    context.Response.Redirect("/swagger/index.html");
                    return Task.CompletedTask;
                });

            app.ApplicationServices.GetRequiredService<TodoDatabase>().Operations.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }
    }
}
