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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

    public class ServiceFabricServer : IServer
    {
        private IServer serverImpl;
        private IServiceProvider serviceProvider;
        private Type serverType;

        private object httpApplicationObj;
        private Type contextType;

        private ICollection<string> addresses = new List<string>();
        private ServiceFabricServerFeatureCollection features;

        public ServiceFabricServer(IServer serverImpl, IServiceProvider serviceProvider)
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
                newServer.Features.Set(feature);
            }

            if (this.addresses.Count > 0 && newServer.Features.Get<IServerAddressesFeature>().Addresses.Count == 0)
            {
                foreach (var address in this.addresses)
                {
                    newServer.Features.Get<IServerAddressesFeature>().Addresses.Add(address);
                }
            }

            this.features.SetFeatures(newServer.Features);
            this.serverImpl = newServer;
        }

        public void Dispose()
        {
            this.serverImpl.Dispose();
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            this.httpApplicationObj = application;
            this.contextType = typeof(TContext);

            foreach (var address in this.serverImpl.Features.Get<IServerAddressesFeature>().Addresses)
            {
                this.addresses.Add(address);
            }

            this.serverImpl.Features.Get<IServerAddressesFeature>().Addresses.Clear();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return this.serverImpl.StopAsync(cancellationToken);
        }

        internal Task StartAsync(CancellationToken cancellationToken)
        {
            return (Task)this.serverType
                .GetMethod("StartAsync")
                .MakeGenericMethod(this.contextType)
                .Invoke(this.serverImpl, new object[] { this.httpApplicationObj, cancellationToken });
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
