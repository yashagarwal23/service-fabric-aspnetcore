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
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class AspNetStatefulService : StatefulService
    {
        private IHost host;
        private ServiceFabricHostOptions hostOptions;
        private List<ServiceReplicaListener> serviceListeners;

        public AspNetStatefulService(StatefulServiceContext serviceContext)
            : base(serviceContext)
        {
        }

        internal void ConfigureHost(IHost host)
        {
            this.host = host;
            this.hostOptions = host.Services.GetRequiredService<IOptions<ServiceFabricHostOptions>>().Value;
        }

        internal void ConfigureListeners(List<ServiceReplicaListener> serviceListeners)
        {
            this.serviceListeners = serviceListeners;
        }

        protected override async Task OnOpenAsync(ReplicaOpenMode openMode, CancellationToken cancellationToken)
        {
            if (!this.hostOptions.HostRunning)
            {
                await this.host.StartAsync(cancellationToken);
                this.hostOptions.NotifyStarted();
            }
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            await this.host.StopAsync(cancellationToken);
            this.hostOptions.NotifyStopped();
            this.host.Dispose();
        }

        protected override void OnAbort()
        {
            this.hostOptions.NotifyStopped();
            this.host.Dispose();
        }

        protected sealed override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.serviceListeners;
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
