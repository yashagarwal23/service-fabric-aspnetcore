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
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatefulServiceBuilder : ServiceBuilder
    {
        private StatefulServiceContext serviceContext;
        private List<Func<StatefulServiceContext, IServiceProvider, ICommunicationListener>> listenerDelegateList = new List<Func<StatefulServiceContext, IServiceProvider, ICommunicationListener>>();
        private List<string> listenerNameList = new List<string>();
        private List<bool> listenOnSecondaryList = new List<bool>();
        private Type serviceType;

        public StatefulServiceBuilder(StatefulServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
            this.ConfigureDefaults();
        }

        public StatefulServiceContext ServiceContext
        {
            get { return this.serviceContext; }
        }

        public StatefulServiceBuilder ConfigureListener(
            Func<StatefulServiceContext, IServiceProvider, ICommunicationListener> listenerDelegate,
            string listenerName = "",
            bool listenOnSecondary = false)
        {
            this.listenerDelegateList.Add(listenerDelegate);
            this.listenerNameList.Add(listenerName);
            this.listenOnSecondaryList.Add(listenOnSecondary);
            return this;
        }

        public StatefulServiceBuilder ConfigureWebHostDefaults(Action<IWebHostBuilder> configure, string listenerName = "WebListener", bool listenOnSecondary = false)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            ((HostBuilder)this).ConfigureWebHostDefaults(webBuilder =>
            {
                configure(webBuilder);
                webBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricServer>());
            });

            this.ConfigureListener(
                (context, provider) =>
                {
                    return new WebCommunicationListener(context, provider);
                },
                listenerName,
                listenOnSecondary);

            return this;
        }

        internal StatefulServiceBuilder UseServiceImplementation(Type serviceType)
        {
            if (typeof(WebStatefulService).IsAssignableFrom(serviceType) == false)
            {
                throw new Exception();
            }

            this.serviceType = serviceType;
            return this;
        }

        internal StatefulService Build(StatefulServiceContext serviceContext)
        {
            this.ConfigureServices(services =>
            {
                services.AddSingleton<ServiceContext>(serviceContext);
                services.AddSingleton<StatefulServiceContext>(serviceContext);
                services.AddSingleton<IReliableStateManager>(provider => provider.GetRequiredService<StatefulService>().StateManager);
            });

            if (this.serviceType != null)
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(this.serviceType);
                    services.AddSingleton(provider => (StatefulService)provider.GetRequiredService(this.serviceType));
                    services.AddSingleton(provider => (WebStatefulService)provider.GetRequiredService(this.serviceType));
                });
            }
            else
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(new WebStatefulService(serviceContext));
                    services.AddSingleton(provider => (StatefulService)provider.GetRequiredService(typeof(WebStatelessService)));
                });
            }

            var host = this.Build();

            var serviceReplicaListeners = new List<ServiceReplicaListener>();
            for (int i = 0; i < this.listenerDelegateList.Count; i++)
            {
                var listener = this.listenerDelegateList[i].Invoke(serviceContext, host.Services);
                serviceReplicaListeners.Add(new ServiceReplicaListener(
                    _ => listener,
                    this.listenerNameList[i],
                    this.listenOnSecondaryList[i]));
            }

            var webStatefulService = host.Services.GetRequiredService<WebStatefulService>();
            webStatefulService.ConfigureListeners(serviceReplicaListeners);

            return webStatefulService;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
