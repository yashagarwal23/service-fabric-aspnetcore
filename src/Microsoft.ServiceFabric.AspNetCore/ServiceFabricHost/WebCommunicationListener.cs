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
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public class WebCommunicationListener : ICommunicationListener
    {
        private readonly IHostBuilder hostBuilder;
        private readonly ServiceContext serviceContext;
        private IHost host;

        public WebCommunicationListener(IHostBuilder hostBuilder, ServiceContext serviceContext)
        {
            this.hostBuilder = hostBuilder;
            this.serviceContext = serviceContext;
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
            if (!this.hostBuilder.Properties.TryGetValue("Host", out var obj))
            {
                throw new InvalidOperationException(SR.HostNullExceptionMessage);
            }

            this.host = (IHost)obj;

            await this.host.StartAsync(cancellationToken);

            var server = this.host.Services.GetService<IServer>();
            if (server == null)
            {
                throw new InvalidOperationException(SR.WebServerNotFound);
            }

            // TODO return all endpoints
            var url = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
            if (url == null)
            {
                throw new InvalidOperationException(SR.ErrorNoUrlFromAspNetCore);
            }

            var publishAddress = this.serviceContext.PublishAddress;

            if (url.Contains("://+:"))
            {
                url = url.Replace("://+:", $"://{publishAddress}:");
            }
            else if (url.Contains("://[::]:"))
            {
                url = url.Replace("://[::]:", $"://{publishAddress}:");
            }

            return url;
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
