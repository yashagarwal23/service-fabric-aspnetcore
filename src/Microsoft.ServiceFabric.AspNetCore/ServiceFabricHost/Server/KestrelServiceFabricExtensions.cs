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
    using Microsoft.AspNetCore.Server.Kestrel.Core;

    public static class KestrelServiceFabricExtensions
    {
        public static IWebHostBuilder UseSFKestrel(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricKestrelServer>());
            return webHostBuilder;
        }

        public static IWebHostBuilder UseSFKestrel(this IWebHostBuilder webHostBuilder, Action<KestrelServerOptions> options)
        {
            webHostBuilder.UseKestrel(options);
            webHostBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricKestrelServer>());
            return webHostBuilder;
        }

        public static IWebHostBuilder UseSFKestrel(this IWebHostBuilder webHostBuilder, Action<WebHostBuilderContext, KestrelServerOptions> configureOptions)
        {
            webHostBuilder.UseKestrel(configureOptions);
            webHostBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricKestrelServer>());
            return webHostBuilder;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
