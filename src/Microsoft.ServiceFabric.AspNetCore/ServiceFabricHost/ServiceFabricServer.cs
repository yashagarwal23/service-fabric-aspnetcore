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

        private ServiceFabricServerFeatureCollection features;

        private IHttpApplication<object> application;

        private ICollection<string> addresses = new List<string>();

        public ServiceFabricServer(IServer serverImpl, IServiceProvider serviceProvider)
        {
            this.serverImpl = serverImpl;
            this.serviceProvider = serviceProvider;
            this.serverType = serverImpl.GetType();

            this.features = new ServiceFabricServerFeatureCollection();
            this.features.ConfigureInternalFeatures(serverImpl.Features);
        }

        public IFeatureCollection Features { get => this.features; }

        public void Reset()
        {
            this.serverImpl.Dispose();

            IServer newServer = (IServer)ActivatorUtilities.CreateInstance(this.serviceProvider, this.serverType);

            this.features.ConfigureInternalFeatures(newServer.Features);

            this.serverImpl = newServer;
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            this.application = new ApplicationWrapper<TContext>(application);

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var address in this.addresses)
            {
                this.Features.Get<IServerAddressesFeature>().Addresses.Add(address);
            }

            await this.serverImpl.StartAsync(this.application, cancellationToken);

            this.addresses = this.Features.Get<IServerAddressesFeature>().Addresses.ToList();
        }

        private class ApplicationWrapper<TContext> : IHttpApplication<object>
        {
            private readonly IHttpApplication<TContext> application;

            public ApplicationWrapper(IHttpApplication<TContext> application)
            {
                this.application = application;
            }

            public void DisposeContext(object context, Exception exception) => this.application.DisposeContext((TContext)context, exception);

            public Task ProcessRequestAsync(object context) => this.application.ProcessRequestAsync((TContext)context);

            public object CreateContext(IFeatureCollection contextFeatures) => this.application.CreateContext(contextFeatures);
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
