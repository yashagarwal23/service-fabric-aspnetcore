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
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class WebStatelessService : StatelessService
    {
        private List<ServiceInstanceListener> serviceListeners;

        public WebStatelessService(StatelessServiceContext serviceContext)
            : base(serviceContext)
        {
        }

        internal void ConfigureListeners(List<ServiceInstanceListener> serviceListeners)
        {
            this.serviceListeners = serviceListeners;
        }

        protected sealed override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.serviceListeners;
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
