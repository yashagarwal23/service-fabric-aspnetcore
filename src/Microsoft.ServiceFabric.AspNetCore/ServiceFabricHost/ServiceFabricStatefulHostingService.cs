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
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class ServiceFabricStatefulHostingService : IHostedService
    {
        private readonly string serviceType;
        private readonly Action<StatefulServiceBuilder> configureBuilder;

        public ServiceFabricStatefulHostingService(string serviceType, Action<StatefulServiceBuilder> configureBuilder)
        {
            this.serviceType = serviceType;
            this.configureBuilder = configureBuilder;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ServiceRuntime.RegisterServiceAsync(
                this.serviceType,
                serviceContext =>
                {
                    StatefulServiceBuilder builder = new StatefulServiceBuilder(serviceContext);
                    this.configureBuilder(builder);
                    return builder.BuildService();
                });
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
