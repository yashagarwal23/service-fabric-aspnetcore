// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1401 // Fields should be private

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;

    internal abstract class ServiceFabricServer : IServer
    {
        protected IServer serverImpl;
        protected IServiceProvider serviceProvider;
        protected Type serverType;

        protected object httpApplicationObj;
        protected Type contextType;

        protected ICollection<string> addresses = new List<string>();
        protected bool preferHostingUrls = false;

        protected ServiceFabricServerFeatureCollection features;

        protected ServiceFabricServer(IServer serverImpl, IServiceProvider serviceProvider)
        {
            this.serverImpl = serverImpl;
            this.serviceProvider = serviceProvider;
            this.serverType = serverImpl.GetType();

            this.features = new ServiceFabricServerFeatureCollection();
            this.features.SetFeatures(this.serverImpl.Features);
        }

        public IFeatureCollection Features { get => this.features; }

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

            this.features.SetFeatures(newServer.Features);
            this.serverImpl = newServer;
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            this.httpApplicationObj = application;
            this.contextType = typeof(TContext);

            foreach (var address in this.serverImpl.Features.Get<IServerAddressesFeature>().Addresses)
            {
                this.addresses.Add(address);
            }

            this.preferHostingUrls = this.serverImpl.Features.Get<IServerAddressesFeature>().PreferHostingUrls;

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

        internal abstract Task StartAsync(CancellationToken cancellationToken);
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1401 // Fields should be private
