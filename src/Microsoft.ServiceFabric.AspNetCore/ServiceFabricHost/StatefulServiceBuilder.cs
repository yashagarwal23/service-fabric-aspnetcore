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
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
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

#if !NET461
        public StatefulServiceBuilder ConfigureWebHostDefaults(
            Action<IWebHostBuilder> configure,
            string listenerName = "",
            bool listenOnSecondary = false,
            ServiceFabricIntegrationOptions serviceFabricIntegrationOptions = ServiceFabricIntegrationOptions.None)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            this.hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                configure(webBuilder);
                webBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricServer>());
                webBuilder.UseServiceFabricIntegration(serviceFabricIntegrationOptions);
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

        public StatefulServiceBuilder ConfigureWebHost(
            Action<IWebHostBuilder> configure,
            string listenerName = "",
            bool listenOnSecondary = false,
            ServiceFabricIntegrationOptions serviceFabricIntegrationOptions = ServiceFabricIntegrationOptions.None)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            this.hostBuilder.ConfigureWebHost(
                webBuilder =>
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

#if NET5_0_OR_GREATER
        public StatefulServiceBuilder ConfigureWebHost(
            Action<IWebHostBuilder> configure,
            Action<WebHostBuilderOptions> configureWebHostBuilder,
            string listenerName = "",
            bool listenOnSecondary = false,
            ServiceFabricIntegrationOptions serviceFabricIntegrationOptions = ServiceFabricIntegrationOptions.None)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (configureWebHostBuilder is null)
            {
                throw new ArgumentNullException(nameof(configureWebHostBuilder));
            }

            this.hostBuilder.ConfigureWebHost(
                webBuilder =>
                {
                    configure(webBuilder);
                    webBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricServer>());
                    webBuilder.UseServiceFabricIntegration(serviceFabricIntegrationOptions);
                },
                configureWebHostBuilder);

            this.ConfigureListener(
                (context, provider) =>
                {
                    return new WebCommunicationListener(context, provider);
                },
                listenerName,
                listenOnSecondary);

            return this;
        }
#endif
#endif

        public StatefulServiceBuilder ConfigureV2RemotingDefaults(bool listenOnSecondary = false)
        {
            if (this.serviceType == null)
            {
                throw new Exception();
            }

            if (typeof(IService).IsAssignableFrom(this.serviceType) == false)
            {
                throw new Exception();
            }

            return this.ConfigureListener(
                (context, provider) =>
                {
                    return new FabricTransportServiceRemotingListener(context, (IService)provider.GetRequiredService(this.serviceType));
                },
                "V2Listener",
                listenOnSecondary);
        }

        public StatefulServiceBuilder ConfigureV2_1RemotingDefaults(bool listenOnSecondary = false)
        {
            if (this.serviceType == null)
            {
                throw new Exception();
            }

            if (typeof(IService).IsAssignableFrom(this.serviceType) == false)
            {
                throw new Exception();
            }

            return this.ConfigureListener(
                (context, provider) =>
                {
                    var settings = new FabricTransportRemotingListenerSettings();
                    settings.UseWrappedMessage = true;
                    return new FabricTransportServiceRemotingListener(context, (IService)provider.GetRequiredService(this.serviceType), settings);
                },
                "V2_1Listener",
                listenOnSecondary);
        }

        internal StatefulServiceBuilder UseServiceImplementation(Type serviceType)
        {
            if (typeof(AspNetStatefulService).IsAssignableFrom(serviceType) == false)
            {
                throw new Exception();
            }

            this.serviceType = serviceType;
            return this;
        }

        internal StatefulService BuildService()
        {
            this.ConfigureServices(services =>
            {
                services.AddSingleton<ServiceContext>(this.serviceContext);
                services.AddSingleton<StatefulServiceContext>(this.serviceContext);
                services.AddSingleton<IReliableStateManager>(provider => provider.GetRequiredService<StatefulService>().StateManager);
            });

            if (this.serviceType != null)
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(this.serviceType);
                    services.AddSingleton(provider => (StatefulService)provider.GetRequiredService(this.serviceType));
                    services.AddSingleton(provider => (AspNetStatefulService)provider.GetRequiredService(this.serviceType));
                });
            }
            else
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(new AspNetStatefulService(this.serviceContext));
                    services.AddSingleton(provider => (StatefulService)provider.GetRequiredService(typeof(AspNetStatelessService)));
                });
            }

            this.ConfigureServices(services => services.AddTransient<IConfigureOptions<ServiceFabricHostOptions>, ServiceFabricHostOptionsSetup>());

            var host = this.hostBuilder.Build();

            var serviceReplicaListeners = new List<ServiceReplicaListener>();
            for (int i = 0; i < this.listenerDelegateList.Count; i++)
            {
                var listener = this.listenerDelegateList[i].Invoke(this.serviceContext, host.Services);
                serviceReplicaListeners.Add(new ServiceReplicaListener(
                    _ => listener,
                    this.listenerNameList[i],
                    this.listenOnSecondaryList[i]));
            }

            var aspNetStatefulService = host.Services.GetRequiredService<AspNetStatefulService>();
            aspNetStatefulService.ConfigureHost(host);
            aspNetStatefulService.ConfigureListeners(serviceReplicaListeners);

            return aspNetStatefulService;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
