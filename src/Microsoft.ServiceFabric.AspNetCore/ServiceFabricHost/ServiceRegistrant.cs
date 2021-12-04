// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;

    internal static class ServiceRegistrant
    {
        private static ConcurrentDictionary<string, IServiceProvider> serviceCollection;

        static ServiceRegistrant()
        {
            serviceCollection = new ConcurrentDictionary<string, IServiceProvider>();
        }

        internal static void Add(ServiceContext serviceContext, IServiceProvider serviceProvider)
        {
            string key = $"/{serviceContext.PartitionId.ToString()}/{serviceContext.ReplicaOrInstanceId.ToString()}";
            serviceCollection.TryAdd(key, serviceProvider);
        }

        internal static IServiceProvider Get(string key)
        {
            return serviceCollection.TryGetValue(key, out var serviceProvider) ? serviceProvider : null;
        }

        internal static IEnumerable<string> GetAll()
        {
            return serviceCollection.ToArray().Select(i => i.Key);
        }

        internal static void Remove(ServiceContext serviceContext)
        {
            string key = $"/{serviceContext.PartitionId.ToString()}/{serviceContext.ReplicaOrInstanceId.ToString()}";
            serviceCollection.TryRemove(key, out var serviceProvider);
        }
    }
}
