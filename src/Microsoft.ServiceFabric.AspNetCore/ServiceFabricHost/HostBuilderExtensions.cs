// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterStatelessService(this IHostBuilder hostBuilder, string serviceType, Action<StatelessServiceBuilder> configureBuilder)
        {
            return hostBuilder.ConfigureServices(parentServices =>
            {
                parentServices.AddSingleton<IHostedService>(parentProvider =>
                {
                    Action<StatelessServiceBuilder> builderConfiguration = (builder) =>
                    {
                        configureBuilder(builder);

                        builder.ConfigureServices(services =>
                        {
                            var included = new HashSet<Type>(services.Select(i => i.ServiceType));
                            foreach (var descriptor in parentServices.Where(i => !included.Contains(i.ServiceType)))
                            {
                                switch (descriptor.Lifetime)
                                {
                                    case ServiceLifetime.Scoped:
                                    case ServiceLifetime.Transient:
                                        services.Add(descriptor);
                                        break;
                                    case ServiceLifetime.Singleton:
                                        if (!descriptor.ServiceType.ContainsGenericParameters)
                                        {
                                            services.AddSingleton(descriptor.ServiceType, _ => parentProvider.GetService(descriptor.ServiceType));
                                        }

                                        break;
                                }
                            }
                        });
                    };

                    return new ServiceFabricStatelessHostingService(serviceType, builderConfiguration);
                });
            });
        }

        public static ICommunicationListener CreateWebCommunicationListener(this IHostBuilder hostBuilder, ServiceContext serviceContext)
        {
            return new WebCommunicationListener(hostBuilder, serviceContext);
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
