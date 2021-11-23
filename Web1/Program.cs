using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Web1
{
    public static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                        webBuilder.UseHttpSys();
                        webBuilder.UseUrls("http://localhost:5000");
                    })
                    .UseServiceFabric()
                    .RegisterStatelessService("Web1Type", sfBuilder =>
                    {
                        sfBuilder.UseEndpoint("ServiceEndpoint1");
                    })
                    .RegisterStatelessService("Web2Type", sfBuilder =>
                    {
                        sfBuilder.UseEndpoint("ServiceEndpoint2");
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
