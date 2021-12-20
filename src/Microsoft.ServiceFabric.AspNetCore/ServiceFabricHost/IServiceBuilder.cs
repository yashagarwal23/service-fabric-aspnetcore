// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System.Fabric;

    public interface IServiceBuilder
    {
        string ServiceType { get; }

        StatelessService Build(StatelessServiceContext serviceContext);
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
