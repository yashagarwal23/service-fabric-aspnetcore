// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;

    public class ServiceFabricHostOptions
    {
        public bool HostRunning { get; private set; }

        internal void NotifyStarted()
        {
            this.HostRunning = true;
        }

        internal void NotifyStopped()
        {
            this.HostRunning = false;
        }
    }
}
