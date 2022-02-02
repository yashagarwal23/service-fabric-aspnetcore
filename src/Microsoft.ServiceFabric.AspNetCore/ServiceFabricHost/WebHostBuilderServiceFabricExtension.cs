// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1601 // Partial elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static partial class WebHostBuilderServiceFabricExtension
    {
        internal static IWebHostBuilder UseServiceFabricIntegration(this IWebHostBuilder hostBuilder, ServiceFabricIntegrationOptions serviceFabricIntegerationOptions)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException("hostBuilder");
            }

            if (hostBuilder.GetSetting(SettingName) == true.ToString())
            {
                return hostBuilder;
            }

            hostBuilder.UseSetting(SettingName, true.ToString());

            if (serviceFabricIntegerationOptions.HasFlag(ServiceFabricIntegrationOptions.UseUniqueServiceUrl))
            {
                hostBuilder.ConfigureServices(services =>
                {
                    services.PostConfigure<ServiceFabricHostOptions>(options => options.ConfigureToUseUniqueServiceUrl());
                });
            }

            hostBuilder.ConfigureServices(services =>
            {
                // Configure MiddleWare
                services.AddSingleton<IStartupFilter>(provider =>
                {
                    var serviceFabricHostOptions = provider.GetService<IOptions<ServiceFabricHostOptions>>().Value;
                    return new ServiceFabricSetupFilter(serviceFabricHostOptions.UrlSuffix, serviceFabricIntegerationOptions);
                });
            });

            return hostBuilder;
        }
    }
}
