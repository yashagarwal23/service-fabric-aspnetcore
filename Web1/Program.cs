using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Web1
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            int randomPort = new Random().Next(5005, 6000);
            int randomPort1 = new Random().Next(5005, 6000);
            try
            {
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>()
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseHttpSys();
                    })
                    .UseServiceFabric()
                    .ConfigureServices(services => services.AddSingleton<IService, PrintService>())
                    .RegisterStatelessService<Web1>("Web1Type", sfBuilder =>
                    {
                        sfBuilder.UseEndpoint(randomPort1);
                    })
                    .RegisterStatelessService<Web1>("Web2Type", sfBuilder =>
                    {
                        sfBuilder.UseEndpoint(randomPort);
                    })
                    .Build();

                host.StartAsync().GetAwaiter().GetResult();
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
