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
    using System.Fabric.Description;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatelessServiceBuilder : ServiceBuilder
    {
        private List<Func<StatelessServiceContext, IServiceProvider, ICommunicationListener>> listenerDelegateList = new List<Func<StatelessServiceContext, IServiceProvider, ICommunicationListener>>();
        private List<string> listenerNameList = new List<string>();
        private StatelessServiceContext serviceContext;

        public StatelessServiceBuilder(StatelessServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
            this.ConfigureDefaults();
        }

        public StatelessServiceContext ServiceContext
        {
            get { return this.serviceContext; }
        }

        public StatelessServiceBuilder ConfigureListener(Func<StatelessServiceContext, IServiceProvider, ICommunicationListener> listenerDelegate, string listenerName = "")
        {
            this.listenerDelegateList.Add(listenerDelegate);
            this.listenerNameList.Add(listenerName);
            return this;
        }

        public EndpointResourceDescription GetEndpointResourceDescription(string endpointName)
        {
            return this.serviceContext.GetEndpointResourceDescription(endpointName);
        }

        internal Microsoft.ServiceFabric.Services.Runtime.StatelessService Build(StatelessServiceContext serviceContext)
        {
            this.ConfigureServices(services =>
            {
                services.AddSingleton<ServiceContext>(serviceContext);
                services.AddSingleton<StatelessServiceContext>(serviceContext);
            });

            var host = this.Build();

            // this.Properties.Add("Host", host);
            var serviceInstanceListenerList = new List<ServiceInstanceListener>();
            for (int i = 0; i < this.listenerDelegateList.Count; i++)
            {
                var listener = this.listenerDelegateList[i].Invoke(serviceContext, host.Services);
                serviceInstanceListenerList.Add(new ServiceInstanceListener(
                    _ => listener,
                    this.listenerNameList[i]));
            }

            return new StatelessService(serviceContext, serviceInstanceListenerList);
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
