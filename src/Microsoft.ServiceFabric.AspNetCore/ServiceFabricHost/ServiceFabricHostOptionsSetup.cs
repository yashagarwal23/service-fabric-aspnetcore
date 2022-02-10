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
    using Microsoft.Extensions.Options;

    internal class ServiceFabricHostOptionsSetup : IConfigureOptions<ServiceFabricHostOptions>
    {
        private readonly ServiceContext serviceContext;

        public ServiceFabricHostOptionsSetup(ServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
        }

        public void Configure(ServiceFabricHostOptions options)
        {
           options.ServiceContext = this.serviceContext;
        }
    }
}
