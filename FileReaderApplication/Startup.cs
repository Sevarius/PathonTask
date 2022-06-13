using System.IO;
using FileReaderApplication.BusinessLogic.Impl;
using FileReaderApplication.BusinessLogic.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

namespace FileReaderApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "FileReaderApplication", Version = "v1"});
                c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "FileReaderApplication.xml"));
            });

            services.AddSingleton<IFileReaderService, FileReaderService>();
            services.AddTransient<IFileReaderFactory, FileReaderFactory>();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileReaderApplication v1"));
            }
            
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseHttpMetrics();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}