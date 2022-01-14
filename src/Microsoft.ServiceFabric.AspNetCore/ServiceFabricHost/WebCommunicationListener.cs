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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public class WebCommunicationListener : ICommunicationListener
    {
        private readonly ServiceContext serviceContext;
        private readonly IServiceProvider serviceProvider;
        private IHost host;

        public WebCommunicationListener(ServiceContext serviceContext, IServiceProvider serviceProvider)
        {
            this.serviceContext = serviceContext;
            this.serviceProvider = serviceProvider;
        }

        public void Abort()
        {
            if (this.host != null)
            {
                this.host.Dispose();
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (this.host != null)
            {
                await this.host.StopAsync(cancellationToken);
                this.host.Dispose();
            }
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            this.host = this.serviceProvider.GetService<IHost>();
            if (this.host == null)
            {
                throw new InvalidOperationException(SR.HostNullExceptionMessage);
            }

            await this.host.StartAsync(cancellationToken);

            var server = this.host.Services.GetService<IServer>();
            if (server == null)
            {
                throw new InvalidOperationException(SR.WebServerNotFound);
            }

            var urls = server.Features.Get<IServerAddressesFeature>().Addresses;
            if (urls == null)
            {
                throw new InvalidOperationException(SR.ErrorNoUrlFromAspNetCore);
            }

            var publishAddress = this.serviceContext.PublishAddress;

            var completeUrls = urls.Select(url =>
            {
                if (url.Contains("://+:"))
                {
                    return url.Replace("://+:", $"://{publishAddress}:");
                }
                else if (url.Contains("://[::]:"))
                {
                    return url.Replace("://[::]:", $"://{publishAddress}:");
                }

                return url;
            });

            return string.Join(";", completeUrls);
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
