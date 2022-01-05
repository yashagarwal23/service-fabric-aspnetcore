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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class ServiceFabricHostingService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceFabricHostingService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var builders = this.serviceProvider.GetService<IEnumerable<StatelessServiceBuilder>>();
            foreach (var builder in builders)
            {
                ServiceRuntime.RegisterServiceAsync(
                    builder.ServiceType,
                    serviceContext =>
                    {
                        var host = builder.Build(serviceContext);
                        return host.Services.GetService<StatelessService>();
                    }).GetAwaiter().GetResult();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
