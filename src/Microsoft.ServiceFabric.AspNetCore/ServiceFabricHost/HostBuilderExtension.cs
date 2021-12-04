// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Runtime;

    public static class HostBuilderExtension
    {
        public static IHostBuilder UseServiceFabric(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService<ServiceFabricHostingService>();
                services.AddSingleton<IStartupFilter>(new ServiceFabricHostStartupFilter());

                services.Decorate<IServer, ServiceFabricServer>();
                services.AddSingleton<IConfigurableServer>(sp => (ServiceFabricServer)sp.GetService<IServer>());
            });
        }

        public static IHostBuilder RegisterStatelessService<TService>(this IHostBuilder hostBuilder, string serviceType, Action<StatelessServiceBuilder> configureBuilder)
            where TService : StatelessService
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(provider =>
                {
                    var builder = new StatelessServiceBuilder(typeof(TService), serviceType, services, provider);
                    configureBuilder(builder);
                    return builder;
                });
            });
        }
    }
}
