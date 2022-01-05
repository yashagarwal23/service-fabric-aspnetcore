// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Runtime;

    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterStatelessService<TService>(this IHostBuilder hostBuilder, string serviceType, Action<IHostBuilder> configureBuilder)
            where TService : StatelessService
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(provider =>
                {
                    var builder = new StatelessServiceBuilder(typeof(TService), serviceType, services, provider, configureBuilder);
                    return builder;
                });
            });
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
