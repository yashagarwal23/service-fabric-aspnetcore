// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class HostingService : IHostedService
    {
        private IServiceProvider ServiceProvider;

        public HostingService(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var builders = this.ServiceProvider.GetService<IEnumerable<StatelessServiceBuilder>>();
            foreach (var builder in builders)
            {
                ServiceRuntime.RegisterServiceAsync(
                    builder.ServiceType,
                    serviceContext =>
                    {
                        this.ServiceProvider.GetService<ServiceContextResolver>().Add(serviceContext);
                        return builder.Build(serviceContext);
                    }).GetAwaiter().GetResult();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
