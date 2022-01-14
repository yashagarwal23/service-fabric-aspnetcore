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
    using System.Fabric.Description;
    using System.Globalization;
    using System.Linq;

    public static class ServiceContextExtensions
    {
        public static EndpointResourceDescription GetEndpointResourceDescription(this ServiceContext serviceContext, string endpointName)
        {
            if (endpointName == null)
            {
                throw new ArgumentNullException("endpointName");
            }

            if (!serviceContext.CodePackageActivationContext.GetEndpoints().Contains(endpointName))
            {
                throw new InvalidOperationException(string.Format(SR.EndpointNameNotFoundExceptionMessage, endpointName));
            }

            return serviceContext.CodePackageActivationContext.GetEndpoint(endpointName);
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
