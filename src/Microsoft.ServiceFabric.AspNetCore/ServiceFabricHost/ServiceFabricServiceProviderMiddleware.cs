// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class ServiceFabricServiceProviderMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IServiceProvider serviceProvider;

        public ServiceFabricServiceProviderMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            this.next = next;
            this.serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.RequestServices = this.serviceProvider.CreateScope().ServiceProvider;
            await this.next(context);
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
