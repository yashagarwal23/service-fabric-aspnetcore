// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Fabric;

    public class StatelessServiceBuilder
    {
        public string ServiceType;
        private IServiceProvider serviceProvider;
        private string endpointName;

        public StatelessServiceBuilder(string serviceType, IServiceProvider serviceProvider)
        {
            this.ServiceType = serviceType;
            this.serviceProvider = serviceProvider;
        }

        public StatelessServiceBuilder UseEndpoint(string endpointName)
        {
            this.endpointName = endpointName;
            return this;
        }

        public StatelessService Build(StatelessServiceContext serviceContext)
        {
            return new StatelessService(serviceContext, this.endpointName, this.serviceProvider);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
