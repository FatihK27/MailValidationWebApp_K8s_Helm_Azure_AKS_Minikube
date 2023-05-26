using Huawei.WebUIMailValidate.Models;
using Huawei.WebUIMailValidate.Services;
using Huawei.WebUIMailValidate.SharedModels;
using Huawei.WebUIMailValidate.StreamHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.IO;
using System.Net;

namespace Huawei.WebUIMailValidate
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var pass = Configuration["RabbitMq:pass"];
            var encpass = WebUtility.UrlEncode(pass);
            var host = Configuration["RabbitMq:ServiceUrl"];
            var user = Configuration["RabbitMq:user"];

            //string password = Configuration.GetValue<string>("RabbitMq:pass");            ;
            //string encodedPassword = WebUtility.UrlEncode(password);
            //string hostname = Configuration.GetValue<string>("RabbitMq:ServiceUrl");
            //string username = Configuration.GetValue<string>("RabbitMq:user");
            int port = 5672;
            //string uristring = $"amqp://{username}:{encodedPassword}@{hostname}:{port}/";
            string uristring = $"amqp://{user}:{encpass}@{host}:{port}/";



            
            services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(uristring), DispatchConsumersAsync = true });
            services.AddTransient<IBufferedFileUploadService, BufferedFileUploadLocalService>();
            services.AddSingleton<RabbitMQPublisher>();
            services.AddSingleton<RabbitMQClientService>();
            services.AddDbContext<AppDbContext>(options =>
            {
                //options.UseNpgsql(Configuration.GetConnectionString("Postgres"));
                options.UseNpgsql(Configuration["ConnectionStrings:Postgres"]);
            });

            services.AddIdentity<User, IdentityRole>(opt =>
            {
                opt.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                app.UseDeveloperExceptionPage();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
