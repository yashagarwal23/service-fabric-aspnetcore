// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Fabric;
    using System.Globalization;

    internal class ServiceFabricHostOptions
    {
        private bool configuredToUseUniqueServiceUrl;

        public ServiceContext ServiceContext { get; set; }

        public bool HostRunning { get; private set; }

        public string UrlSuffix { get; private set; } = string.Empty;

        internal void NotifyStarted()
        {
            this.HostRunning = true;
        }

        internal void NotifyStopped()
        {
            this.HostRunning = false;
        }

        internal void ConfigureToUseUniqueServiceUrl()
        {
            if (!this.configuredToUseUniqueServiceUrl)
            {
                this.UrlSuffix = string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", this.ServiceContext.PartitionId, this.ServiceContext.ReplicaOrInstanceId);

                if (this.ServiceContext is StatefulServiceContext)
                {
                    // For stateful service, also append a Guid, Guid makes the url unique in scenarios for stateful services when Listener is
                    // created to support read on secondary and change role happens from Primary->Secondary for the replica.
                    this.UrlSuffix += "/" + Guid.NewGuid();
                }

                this.configuredToUseUniqueServiceUrl = true;
            }
        }
    }
}
