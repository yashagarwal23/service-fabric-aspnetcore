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

#pragma warning disable SA1600 // Elements should be documented

    public interface IParentServiceProvider : IServiceProvider
    {
    }

    public class ParentServiceProvider : IParentServiceProvider
    {
        private IServiceProvider serviceProvider;

        public ParentServiceProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented

