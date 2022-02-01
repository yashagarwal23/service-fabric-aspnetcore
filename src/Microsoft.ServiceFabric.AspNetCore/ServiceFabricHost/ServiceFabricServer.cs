// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceFabricServer : IServer
    {
        private IServer serverImpl;
        private IServiceProvider serviceProvider;
        private Type serverType;

        private object httpApplicationObj;
        private Type contextType;

        private ICollection<string> addresses = new List<string>();

        public ServiceFabricServer(IServer serverImpl, IServiceProvider serviceProvider)
        {
            this.serverImpl = serverImpl;
            this.serviceProvider = serviceProvider;
            this.serverType = serverImpl.GetType();
        }

        public IFeatureCollection Features { get => this.serverImpl.Features; }

        public void Reset()
        {
            IServer newServer = (IServer)ActivatorUtilities.CreateInstance(this.serviceProvider, this.serverType);
            foreach (var feature in this.Features)
            {
                if (feature.Key != typeof(IServerAddressesFeature))
                {
                    typeof(IFeatureCollection)
                    .GetMethod("Set")
                    .MakeGenericMethod(feature.Key)
                    .Invoke(newServer.Features, new object[] { feature.Value });
                }
            }

            this.serverImpl = newServer;
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            this.httpApplicationObj = application;
            this.contextType = typeof(TContext);

            this.addresses = this.serverImpl.Features.Get<IServerAddressesFeature>().Addresses.ToList();

            this.serverImpl.Features.Get<IServerAddressesFeature>().Addresses.Clear();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.serverImpl.StopAsync(cancellationToken);
            this.Dispose();
        }

        public void Dispose()
        {
            this.serverImpl.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var address in this.addresses)
            {
                this.Features.Get<IServerAddressesFeature>().Addresses.Add(address);
            }

            return (Task)this.serverType
               .GetMethod("StartAsync")
               .MakeGenericMethod(this.contextType)
               .Invoke(this.serverImpl, new object[] { this.httpApplicationObj, cancellationToken });
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
