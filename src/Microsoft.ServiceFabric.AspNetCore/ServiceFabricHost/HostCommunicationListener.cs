// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

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

    internal class HostCommunicationListener : ICommunicationListener
    {
        private ServiceContext serviceContext;
        private IHost host;

        internal HostCommunicationListener(ServiceContext serviceContext, IHost host)
        {
            this.serviceContext = serviceContext;
            this.host = host;
        }

        public void Abort()
        {
            this.host.Dispose();
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await this.host.StopAsync();
            this.host.Dispose();
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            await this.host.StartAsync();

            var server = this.host.Services.GetService<IServer>();
            var url = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();

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
