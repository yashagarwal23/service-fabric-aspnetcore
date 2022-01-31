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
    using Microsoft.AspNetCore.Hosting.Server.Features;

    internal class ServiceFabricHttpSysServer : ServiceFabricServer
    {
        private bool addressFeaturesConfigured = false;

        public ServiceFabricHttpSysServer(IServer serverImpl, IServiceProvider serviceProvider)
            : base(serverImpl, serviceProvider)
        {
        }

        internal override Task StartAsync(CancellationToken cancellationToken)
        {
            if (!this.addressFeaturesConfigured)
            {
                foreach (var address in this.addresses)
                {
                    this.Features.Get<IServerAddressesFeature>().Addresses.Add(address);
                }

                this.Features.Get<IServerAddressesFeature>().PreferHostingUrls = this.preferHostingUrls;

                this.addressFeaturesConfigured = true;
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
