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
    using Microsoft.AspNetCore.Hosting.Server.Features;

    internal class ServiceFabricServerAddressFeature : IServerAddressesFeature
    {
        private IServerAddressesFeature internalAddressFeatures = null;

        public ICollection<string> Addresses
        {
            get
            {
                if (this.internalAddressFeatures == null)
                {
                    return null;
                }

                return this.internalAddressFeatures.Addresses;
            }
        }

        public bool PreferHostingUrls
        {
            get { return this.internalAddressFeatures.PreferHostingUrls; }
            set { this.internalAddressFeatures.PreferHostingUrls = value; }
        }

        internal void ConfigureInternalAddressFeatures(IServerAddressesFeature serverAddressesFeature)
        {
            this.internalAddressFeatures = serverAddressesFeature;
        }
    }
}
