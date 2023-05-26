using Huawei.WebUIMailValidate.Models;
using Huawei.WebUIMailValidate.SharedModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Huawei.WebUIMailValidate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

            try
            {
                Log.Information("Starting web application");
                var host = CreateHostBuilder(args).Build();
                
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                using (var scope = host.Services.CreateScope())
                {
                    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    appDbContext.Database.Migrate();
                    if (!appDbContext.Users.Any())
                    {
                        userManager.CreateAsync(new User() { UserName = "deneme", Email = "deneme@outlook.com" }, "123").Wait();
                        userManager.CreateAsync(new User() { UserName = "deneme2", Email = "deneme2@outlook.com" }, "Password12*").Wait();
                    }
                }
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally 
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration(
                (hostContext, config) =>
                {
                    //config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddCommandLine(args);
                    config.AddEnvironmentVariables();
                })
            ;
    }
}
