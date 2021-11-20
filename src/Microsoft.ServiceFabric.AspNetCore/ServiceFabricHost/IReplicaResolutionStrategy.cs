// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    internal interface IReplicaResolutionStrategy
    {
        Task<string> GetReplicaIdentifierAsync();
    }

    internal class ReplicaTenantResolutionStrategy : IReplicaResolutionStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReplicaTenantResolutionStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetReplicaIdentifierAsync()
        {
            return await Task.FromResult(this._httpContextAccessor.HttpContext.Request.Host.Port.ToString());
        }
    }
}
