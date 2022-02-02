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
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatelessServiceBuilder : ServiceBuilder
    {
        private List<Func<StatelessServiceContext, IServiceProvider, ICommunicationListener>> listenerDelegateList = new List<Func<StatelessServiceContext, IServiceProvider, ICommunicationListener>>();
        private List<string> listenerNameList = new List<string>();
        private StatelessServiceContext serviceContext;
        private Type serviceType;

        public StatelessServiceBuilder(StatelessServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
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

#if !NET461
        public StatelessServiceBuilder ConfigureWebHostDefaults(
            Action<IWebHostBuilder> configure,
            string listenerName = "",
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
                listenerName);
            return this;
        }

        public StatelessServiceBuilder ConfigureWebHost(
            Action<IWebHostBuilder> configure,
            string listenerName = "",
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
                    webBuilder.UseServiceFabricIntegration(serviceFabricIntegrationOptions);
                });

            this.ConfigureListener(
                (context, provider) =>
                {
                    return new WebCommunicationListener(context, provider);
                },
                listenerName);

            return this;
        }

#if NET5_0_OR_GREATER
        public StatelessServiceBuilder ConfigureWebHost(
            Action<IWebHostBuilder> configure,
            Action<WebHostBuilderOptions> configureWebHostBuilder,
            string listenerName = "",
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
                listenerName);

            return this;
        }
#endif
#endif

        public StatelessServiceBuilder ConfigureV2RemotingDefaults()
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
                "V2Listener");
        }

        public StatelessServiceBuilder ConfigureV2_1RemotingDefaults()
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
                "V2_1Listener");
        }

        internal StatelessServiceBuilder UseServiceImplementation(Type serviceType)
        {
            if (typeof(AspNetStatelessService).IsAssignableFrom(serviceType) == false)
            {
                throw new Exception();
            }

            this.serviceType = serviceType;
            return this;
        }

        internal StatelessService BuildService()
        {
            this.ConfigureServices(services =>
            {
                services.AddSingleton<ServiceContext>(this.serviceContext);
                services.AddSingleton<StatelessServiceContext>(this.serviceContext);
            });

            if (this.serviceType != null)
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(this.serviceType);
                    services.AddSingleton(provider => (StatelessService)provider.GetRequiredService(this.serviceType));
                    services.AddSingleton(provider => (AspNetStatelessService)provider.GetRequiredService(this.serviceType));
                });
            }
            else
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(new AspNetStatelessService(this.serviceContext));
                    services.AddSingleton(provider => (StatelessService)provider.GetRequiredService(typeof(AspNetStatelessService)));
                });
            }

            this.ConfigureServices(services =>
            {
                services.AddTransient<IStatelessServicePartition>(provider => provider.GetRequiredService<AspNetStatelessService>().GetPartition());
                services.AddTransient<IServicePartition>(provider => provider.GetRequiredService<AspNetStatelessService>().GetPartition());
            });

            this.ConfigureServices(services => services.AddTransient<IConfigureOptions<ServiceFabricHostOptions>, ServiceFabricHostOptionsSetup>());

            var host = this.hostBuilder.Build();

            var serviceInstanceListeners = new List<ServiceInstanceListener>();
            for (int i = 0; i < this.listenerDelegateList.Count; i++)
            {
                var listener = this.listenerDelegateList[i].Invoke(this.serviceContext, host.Services);
                serviceInstanceListeners.Add(new ServiceInstanceListener(
                    _ => listener,
                    this.listenerNameList[i]));
            }

            var aspNetStatelessService = host.Services.GetRequiredService<AspNetStatelessService>();
            aspNetStatelessService.ConfigureHost(host);
            aspNetStatelessService.ConfigureListeners(serviceInstanceListeners);

            return aspNetStatelessService;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
