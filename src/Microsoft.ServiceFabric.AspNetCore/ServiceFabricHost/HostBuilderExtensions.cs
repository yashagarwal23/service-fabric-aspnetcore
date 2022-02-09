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
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterStatelessService(this IHostBuilder hostBuilder, string serviceType, Action<StatelessServiceBuilder> configureBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    Action<StatelessServiceBuilder> builderConfiguration = (builder) =>
                    {
                        configureBuilder(builder);
                        ConfigureGlobalHostSettings(builder, services, provider);
                    };

                    return new ServiceFabricStatelessHostingService(serviceType, builderConfiguration);
                });
            });
        }

        public static IHostBuilder RegisterStatelessService<TService>(this IHostBuilder hostBuilder, string serviceType, Action<StatelessServiceBuilder> configureBuilder)
            where TService : AspNetStatelessService
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    Action<StatelessServiceBuilder> builderConfiguration = (builder) =>
                    {
                        builder.UseServiceImplementation(typeof(TService));
                        configureBuilder(builder);
                        ConfigureGlobalHostSettings(builder, services, provider);
                    };

                    return new ServiceFabricStatelessHostingService(serviceType, builderConfiguration);
                });
            });
        }

        public static IHostBuilder RegisterStatefulService(this IHostBuilder hostBuilder, string serviceType, Action<StatefulServiceBuilder> configureBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    Action<StatefulServiceBuilder> builderConfiguration = (builder) =>
                    {
                        configureBuilder(builder);
                        ConfigureGlobalHostSettings(builder, services, provider);
                    };

                    return new ServiceFabricStatefulHostingService(serviceType, builderConfiguration);
                });
            });
        }

        public static IHostBuilder RegisterStatefulService<TService>(this IHostBuilder hostBuilder, string serviceType, Action<StatefulServiceBuilder> configureBuilder)
            where TService : AspNetStatefulService
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IHostedService>(provider =>
                {
                    Action<StatefulServiceBuilder> builderConfiguration = (builder) =>
                    {
                        builder.UseServiceImplementation(typeof(TService));
                        configureBuilder(builder);
                        ConfigureGlobalHostSettings(builder, services, provider);
                    };

                    return new ServiceFabricStatefulHostingService(serviceType, builderConfiguration);
                });
            });
        }

        private static void ConfigureGlobalHostSettings(ServiceBuilder sfBuilder, IServiceCollection globalServices, IServiceProvider globalProvider)
        {
            // Configure global DI Services
            sfBuilder.ConfigureServices(services =>
            {
                var servicesIgnored = new HashSet<Type>(services.Select(i => i.ServiceType));
                servicesIgnored.Add(typeof(IHostedService));

                foreach (var descriptor in globalServices.Where(i => !servicesIgnored.Contains(i.ServiceType)))
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
                                services.AddSingleton(descriptor.ServiceType, _ => globalProvider.GetService(descriptor.ServiceType));
                            }

                            break;
                    }
                }
            });

            // Configure global Configuration
            sfBuilder.ConfigureHostConfiguration(config =>
            {
                config.AddConfiguration(globalProvider.GetRequiredService<IConfiguration>());
            });
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
