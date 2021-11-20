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

    internal class ServiceContextResolver
    {
        private readonly IReplicaResolutionStrategy replicaResolver;
        private Dictionary<string, object> store;

        public ServiceContextResolver(IReplicaResolutionStrategy replicaResolver)
        {
            this.store = new Dictionary<string, object>();
            this.replicaResolver = replicaResolver;
        }

        internal void Add(StatelessServiceContext serviceContext)
        {
            this.store.Add(serviceContext.InstanceId.ToString(), serviceContext);
        }

        internal void Add(string replicaId, StatelessServiceContext serviceContext)
        {
            this.store.Add(replicaId, serviceContext);
        }

        internal StatelessServiceContext GetCurrentServiceContext()
        {
            return (StatelessServiceContext)this.store[this.replicaResolver.GetReplicaIdentifierAsync().GetAwaiter().GetResult()];
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
