// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Runtime.Versioning;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Server.HttpSys;

#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static class HttpSysServiceFabricExtensions
    {
        public static IWebHostBuilder UseSFHttpSys(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.UseHttpSys();
            webHostBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricHttpSysServer>());
            return webHostBuilder;
        }

        public static IWebHostBuilder UseSFHttpSys(this IWebHostBuilder webHostBuilder, Action<HttpSysOptions> options)
        {
            webHostBuilder.UseHttpSys(options);
            webHostBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricHttpSysServer>());
            return webHostBuilder;
        }
    }
}


#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
