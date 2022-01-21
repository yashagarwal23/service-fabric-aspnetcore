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
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
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

        public StatelessServiceBuilder ConfigureWebHostDefaults(
            Action<IWebHostBuilder> configure,
            string listenerName = "")
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
                listenerName);
            return this;
        }

        public StatelessServiceBuilder ConfigureWebHost(
            Action<IWebHostBuilder> configure,
            string listenerName = "")
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            ((HostBuilder)this).ConfigureWebHost(
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
                listenerName);

            return this;
        }

#if NET5_0_OR_GREATER
        public StatelessServiceBuilder ConfigureWebHost(
            Action<IWebHostBuilder> configure,
            Action<WebHostBuilderOptions> configureWebHostBuilder,
            string listenerName = "")
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (configureWebHostBuilder is null)
            {
                throw new ArgumentNullException(nameof(configureWebHostBuilder));
            }

            ((HostBuilder)this).ConfigureWebHost(
                webBuilder =>
                {
                    configure(webBuilder);
                    webBuilder.ConfigureServices(services => services.Decorate<IServer, ServiceFabricServer>());
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

        internal StatelessServiceBuilder UseServiceImplementation(Type serviceType)
        {
            if (typeof(WebStatelessService).IsAssignableFrom(serviceType) == false)
            {
                throw new Exception();
            }

            this.serviceType = serviceType;
            return this;
        }

        internal StatelessService Build(StatelessServiceContext serviceContext)
        {
            this.ConfigureServices(services =>
            {
                services.AddSingleton<ServiceContext>(serviceContext);
                services.AddSingleton<StatelessServiceContext>(serviceContext);
            });

            if (this.serviceType != null)
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(this.serviceType);
                    services.AddSingleton(provider => (StatelessService)provider.GetRequiredService(this.serviceType));
                    services.AddSingleton(provider => (WebStatelessService)provider.GetRequiredService(this.serviceType));
                });
            }
            else
            {
                this.ConfigureServices(services =>
                {
                    services.AddSingleton(new WebStatelessService(serviceContext));
                    services.AddSingleton(provider => (StatelessService)provider.GetRequiredService(typeof(WebStatelessService)));
                });
            }

            this.ConfigureServices(services =>
            {
                var descriptor = services.LastOrDefault(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType.Name == "GenericWebHostService");
                services.Remove(descriptor);

                services.AddSingleton(provider =>
                {
                    var impl = (IHostedService)ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);
                    return new ServiceFabricGenericWebHostService(impl);
                });
            });

            var host = this.Build();

            var serviceInstanceListeners = new List<ServiceInstanceListener>();
            for (int i = 0; i < this.listenerDelegateList.Count; i++)
            {
                var listener = this.listenerDelegateList[i].Invoke(serviceContext, host.Services);
                serviceInstanceListeners.Add(new ServiceInstanceListener(
                    _ => listener,
                    this.listenerNameList[i]));
            }

            var webStatelessService = host.Services.GetRequiredService<WebStatelessService>();
            webStatelessService.ConfigureHost(host);
            webStatelessService.ConfigureListeners(serviceInstanceListeners);

            return webStatelessService;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
