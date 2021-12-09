// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable IDE0052 // Remove unread private members

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Server.HttpSys;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    internal enum ServerType
    {
        Kestrel,
        HttpSys,
    }

    public class ServiceFabricServer : IServer, IConfigurableServer
    {
        private IServer server;
        private IServiceProvider serviceProvider;
        private bool isUrlAdded = false;

        public ServiceFabricServer(IServer server, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.server = server;
        }

        public IFeatureCollection Features
        {
            get
            {
                return this.server.Features;
            }
        }

        public void Add(int port)
        {
            var options = this.serviceProvider.GetService<IOptions<HttpSysOptions>>().Value;
            var addresses = options.UrlPrefixes;
            addresses.Add($"http://localhost:{port}");
            this.isUrlAdded = true;
        }

        public void Dispose()
        {
            this.server.Dispose();
        }

        public void Remove(int port)
        {
            var options = this.serviceProvider.GetService<IOptions<HttpSysOptions>>().Value;
            var addresses = options.UrlPrefixes;
            addresses.Remove($"http://localhost:{port}");
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            var options = this.serviceProvider.GetService<IOptions<HttpSysOptions>>().Value;
            options.UrlPrefixes.Clear();
            this.Features.Get<IServerAddressesFeature>().Addresses.Clear();

            var task = Task.Run(() =>
            {
                while (!this.isUrlAdded)
                {
                    Thread.Sleep(1000);
                }

                this.server.StartAsync<TContext>(application, cancellationToken);
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // this.GetServerImpl();
            return this.server.StopAsync(cancellationToken);
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore IDE0052 // Remove unread private members
