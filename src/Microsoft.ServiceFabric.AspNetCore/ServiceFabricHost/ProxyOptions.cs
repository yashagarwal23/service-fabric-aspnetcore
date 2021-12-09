// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

#pragma warning disable SA1600 // Elements should be documented
    public class ProxyOptions<T> : IOptions<T>
        where T : class, new()
    {
        private IParentServiceProvider serviceProvider;

        public ProxyOptions(IParentServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public T Value { get => this.serviceProvider.GetService<IOptions<T>>().Value; }
    }
}
#pragma warning restore SA1600 // Elements should be documented

