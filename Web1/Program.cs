using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Web1
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            Host.CreateDefaultBuilder()
                .RegisterStatelessService("Web1Type", sfbuilder =>
                {
                    sfbuilder.UseServiceImplementation(typeof(Web1));

                    var httpEndpoint = sfbuilder.ServiceContext.GetEndpointResourceDescription("Web1ServiceEndpoint");
                    var httpsEndpoint = sfbuilder.ServiceContext.GetEndpointResourceDescription("Web1ServiceEndpointHttps");
                    var listenUrl = string.Format(CultureInfo.InvariantCulture, "{0}://+:{1}", httpEndpoint.Protocol.ToString().ToLowerInvariant(), httpEndpoint.Port);
                    var listenUrl2 = string.Format(CultureInfo.InvariantCulture, "{0}://+:{1}", httpsEndpoint.Protocol.ToString().ToLowerInvariant(), httpsEndpoint.Port);

                    sfbuilder.ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls(string.Join(";", listenUrl, listenUrl2));
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseKestrel();
                    });

                    sfbuilder.ConfigureListener(
                        (context, provider) =>
                        {
                            return new WebCommunicationListener(context, provider);
                        },
                        "WebListener")
                    .ConfigureListener(
                        (context, provider) =>
                        {
                            return new FabricTransportServiceRemotingListener(context, provider.GetRequiredService<Web1>());
                        },
                        "V2Listener");
                })
                .RegisterStatelessService("Web2Type", sfbuilder =>
                {
                    var httpEndpoint = sfbuilder.ServiceContext.GetEndpointResourceDescription("Web2ServiceEndpoint");
                    var listenUrl = string.Format(CultureInfo.InvariantCulture, "{0}://+:{1}", httpEndpoint.Protocol.ToString().ToLowerInvariant(), httpEndpoint.Port);

                    sfbuilder.ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls(listenUrl);
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseKestrel();
                    });

                    sfbuilder.ConfigureListener((context, provider) =>
                    {
                        return new WebCommunicationListener(context, provider);
                    });
                })
                .ConfigureServices(services => services.AddSingleton<PrintService>())
                .Build()
                .Run();
        }

        /// <summary>
        /// Finds the ASP .NET Core HTTPS development certificate in development environment. Update this method to use the appropriate certificate for production environment.
        /// </summary>
        /// <returns>Returns the ASP .NET Core HTTPS development certificate</returns>
        private static X509Certificate2 GetCertificateFromStore()
        {
            string aspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.Equals(aspNetCoreEnvironment, "Development", StringComparison.OrdinalIgnoreCase))
            {
                const string aspNetHttpsOid = "1.3.6.1.4.1.311.84.1.1";
                const string CNName = "CN=localhost";
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certCollection = store.Certificates;
                    var currentCerts = certCollection.Find(X509FindType.FindByExtension, aspNetHttpsOid, true);
                    currentCerts = currentCerts.Find(X509FindType.FindByIssuerDistinguishedName, CNName, true);
                    return currentCerts.Count == 0 ? null : currentCerts[0];
                }
            }
            else
            {
                throw new NotImplementedException("GetCertificateFromStore should be updated to retrieve the certificate for non Development environment");
            }
        }
    }
}
