// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Fabric;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public class WebCommunicationListener : ICommunicationListener
    {
        private readonly ServiceContext serviceContext;
        private IHost host;
        private ServiceFabricServer server;
        private ServiceFabricHostOptions hostOptions;

        public WebCommunicationListener(ServiceContext serviceContext, IServiceProvider serviceProvider)
        {
            this.serviceContext = serviceContext;
            this.host = serviceProvider.GetRequiredService<IHost>();
            this.server = (ServiceFabricServer)serviceProvider.GetService<IServer>();
            this.hostOptions = serviceProvider.GetService<IOptions<ServiceFabricHostOptions>>().Value;
        }

        public void Abort()
        {
            this.server.Dispose();
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await this.server.StopAsync(cancellationToken);
            this.server.Dispose();
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            if (!this.hostOptions.HostRunning)
            {
                await this.host.StartAsync(cancellationToken);
                this.hostOptions.NotifyStarted();
            }

            if (this.server == null)
            {
                throw new InvalidOperationException(SR.WebServerNotFound);
            }

            this.server.Reset();
            await this.server.StartAsync(cancellationToken);

            var url = this.server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
            if (url == null)
            {
                throw new InvalidOperationException(SR.ErrorNoUrlFromAspNetCore);
            }

            var urlSuffix = this.hostOptions.UrlSuffix;
            var publishAddress = this.serviceContext.PublishAddress;

            if (url.Contains("://+:"))
            {
                url = url.Replace("://+:", $"://{publishAddress}:");
            }
            else if (url.Contains("://[::]:"))
            {
                url = url.Replace("://[::]:", $"://{publishAddress}:");
            }
            else if (url.Contains("://0.0.0.0:"))
            {
                url = url.Replace("://0.0.0.0:", $"://{publishAddress}:");
            }

            return url.TrimEnd(new[] { '/' }) + urlSuffix;
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
