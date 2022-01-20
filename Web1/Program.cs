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
                .ConfigureServices(services => services.AddSingleton<PrintService>())
                .RegisterStatelessService("Web1Type", sfbuilder =>
                {
                    sfbuilder.UseServiceImplementation(typeof(Web1));

                    var httpEndpoint = sfbuilder.ServiceContext.GetEndpointResourceDescription("Web1ServiceEndpoint");
                    var httpsEndpoint = sfbuilder.ServiceContext.GetEndpointResourceDescription("Web1ServiceEndpointHttps");

                    sfbuilder.ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseKestrel(opt =>
                        {
                            opt.Listen(IPAddress.IPv6Any, httpsEndpoint.Port, listenOptions =>
                            {
                                listenOptions.UseHttps(FindMatchingCertificateBySubject("mytestcert"));
                            });
                            opt.Listen(IPAddress.IPv6Any, httpEndpoint.Port);
                        });
                    })
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
                })
                .Build()
                .Run();
        }

        /// <summary>
        /// Finds the ASP .NET Core HTTPS development certificate in development environment. Update this method to use the appropriate certificate for production environment.
        /// </summary>
        /// <returns>Returns the ASP .NET Core HTTPS development certificate</returns>
        private static X509Certificate2 FindMatchingCertificateBySubject(string subjectCommonName)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                var certCollection = store.Certificates;
                var matchingCerts = new X509Certificate2Collection();

                foreach (var enumeratedCert in certCollection)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(subjectCommonName, enumeratedCert.GetNameInfo(X509NameType.SimpleName, forIssuer: false))
                      && DateTime.Now < enumeratedCert.NotAfter
                      && DateTime.Now >= enumeratedCert.NotBefore)
                    {
                        matchingCerts.Add(enumeratedCert);
                    }
                }

                if (matchingCerts.Count == 0)
                {
                    throw new Exception($"Could not find a match for a certificate with subject 'CN={subjectCommonName}'.");
                }

                return matchingCerts[0];
            }
        }
    }
}
