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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatelessServiceBuilder : ServiceBuilder
    {
        private string serviceType;
        private IServiceCollection services;
        private IServiceProvider provider;
        private List<Func<StatelessServiceContext, HostBuilder, ICommunicationListener>> listenerDelegateList = new List<Func<StatelessServiceContext, HostBuilder, ICommunicationListener>>();
        private List<string> listenerNameList = new List<string>();

        public StatelessServiceBuilder(string serviceType, IServiceCollection services, IServiceProvider provider)
        {
            this.serviceType = serviceType;
            this.services = services;
            this.provider = provider;

            this.ConfigureDefaults();
        }

        public StatelessServiceBuilder()
        {
            this.ConfigureDefaults();
        }

        public StatelessServiceBuilder ConfigureListener(Func<StatelessServiceContext, HostBuilder, ICommunicationListener> listenerDelegate, string listenerName = "")
        {
            this.listenerDelegateList.Add(listenerDelegate);
            this.listenerNameList.Add(listenerName);
            return this;
        }

        internal Microsoft.ServiceFabric.Services.Runtime.StatelessService Build(StatelessServiceContext serviceContext)
        {
            var serviceInstanceListenerList = new List<ServiceInstanceListener>();
            for (int i = 0; i < this.listenerDelegateList.Count; i++)
            {
                var listener = this.listenerDelegateList[i].Invoke(serviceContext, this);
                serviceInstanceListenerList.Add(new ServiceInstanceListener(
                    _ => listener,
                    this.listenerNameList[i]));
            }

            this.ConfigureServices(services =>
            {
                services.AddSingleton<ServiceContext>(serviceContext);
                services.AddSingleton<StatelessServiceContext>(serviceContext);
            });

            var host = this.Build();
            this.Properties.Add("Host", host);

            return new StatelessService(serviceContext, serviceInstanceListenerList);
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
