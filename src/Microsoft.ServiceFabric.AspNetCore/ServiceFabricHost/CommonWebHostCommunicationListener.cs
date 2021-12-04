// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Server.HttpSys;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore.ServiceFabricHost;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

#pragma warning disable SA1600 // Elements should be documented
    public class CommonWebHostCommunicationListener : ICommunicationListener
    {
        private ServiceContext serviceContext;
        private IServiceProvider serviceProvider;
        private ListenerOptions listenerOptions;
        private int listenPort;
        private IConfigurableServer configurableServer;

        public CommonWebHostCommunicationListener(IServiceProvider serviceProvider)
        {
            this.serviceContext = serviceProvider.GetService<ServiceContext>();
            this.serviceProvider = serviceProvider;
            this.listenerOptions = serviceProvider.GetService<ListenerOptions>();
            this.listenPort = this.listenerOptions.EndPoint;
            this.configurableServer = serviceProvider.GetService<IConfigurableServer>();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.configurableServer.Remove(this.listenPort);
            ServiceRegistrant.Remove(this.serviceContext);

            return Task.CompletedTask;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            this.configurableServer.Add(this.listenPort);
            var publishAddress = this.serviceContext.PublishAddress;
            var listenUrl = $"http://+:{this.listenPort}";
            listenUrl = listenUrl.Replace("://+:", $"://{publishAddress}:");

            ServiceRegistrant.Add(this.serviceContext, this.serviceProvider);

            var fullAddress = listenUrl + $"/{this.serviceContext.PartitionId}/{this.serviceContext.ReplicaOrInstanceId}";
            return Task.FromResult(fullAddress);
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
