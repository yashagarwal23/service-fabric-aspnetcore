// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Fabric;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;

    public static class ServiceFabricHostExtensions
    {
        public static IHostBuilder UseServiceFabric(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService<HostingService>();
                services.AddSingleton<IReplicaResolutionStrategy, ReplicaTenantResolutionStrategy>();
                services.AddSingleton<ServiceContextResolver>();
                services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                services.AddScoped<StatelessServiceContext>(provider => provider.GetRequiredService<ServiceContextResolver>().GetCurrentServiceContext());
            });
        }

        public static IHostBuilder RegisterStatelessService(this IHostBuilder hostBuilder, string serviceType, Action<StatelessServiceBuilder> configureBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(provider =>
                {
                    var builder = new StatelessServiceBuilder(serviceType, provider);
                    configureBuilder(builder);
                    return builder;
                });
            });
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
