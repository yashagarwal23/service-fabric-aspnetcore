// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceFabricServer : IServer
    {
        private IServiceProvider serviceProvider;
        private IServer server;
        private Type serverType;

        public ServiceFabricServer(IServer server, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.server = server;
            this.serverType = server.GetType();
        }

        public IFeatureCollection Features { get => this.server.Features; }

        public void Dispose()
        {
            if (this.server != null)
            {
                this.server.Dispose();
            }
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            return this.server.StartAsync<TContext>(application, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.server.StopAsync(cancellationToken);
            this.server.Dispose();

            this.server = (IServer)ActivatorUtilities.CreateInstance(this.serviceProvider, this.serverType);
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
