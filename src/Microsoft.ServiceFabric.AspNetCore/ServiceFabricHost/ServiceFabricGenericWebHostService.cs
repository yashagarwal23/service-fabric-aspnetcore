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

    internal class ServiceFabricGenericWebHostService : IHostedService
    {
        private IHostedService genericWebHostServiceImpl;

        public ServiceFabricGenericWebHostService(IHostedService genericWebHostServiceImpl)
        {
            this.genericWebHostServiceImpl = genericWebHostServiceImpl;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return this.genericWebHostServiceImpl.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return this.genericWebHostServiceImpl.StopAsync(cancellationToken);
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or membe
