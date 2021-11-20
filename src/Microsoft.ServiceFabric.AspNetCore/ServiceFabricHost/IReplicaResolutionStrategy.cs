// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    internal interface IReplicaResolutionStrategy
    {
        IDictionary<string, string> ReplicaStore { get; }

        void MapPort(string port, string replicaId);

        Task<string> GetReplicaIdentifierAsync();
    }

    internal class ReplicaTenantResolutionStrategy : IReplicaResolutionStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReplicaTenantResolutionStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            this.ReplicaStore = new ConcurrentDictionary<string, string>();
        }

        public IDictionary<string, string> ReplicaStore { get; }

        public void MapPort(string port, string replicaId)
        {
            this.ReplicaStore.Add(port, replicaId);
        }

        public async Task<string> GetReplicaIdentifierAsync()
        {
            return await Task.FromResult(this.ReplicaStore[this._httpContextAccessor.HttpContext.Request.Host.Port.ToString()]);
        }
    }
}
