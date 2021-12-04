// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Server.HttpSys;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore.ServiceFabricHost;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatelessServiceBuilder
    {
        private Type serviceClass;
        private IServiceCollection parentServices;
        private IServiceProvider parentProvider;
        private List<Action<IServiceCollection>> configureServiceActions = new List<Action<IServiceCollection>>();
        private IServiceProvider serviceProvider;
        private ListenerOptions listenerOptions = null;

        internal StatelessServiceBuilder(Type serviceClass, string serviceType, IServiceCollection parentServices, IServiceProvider parentProvider)
        {
            this.serviceClass = serviceClass;
            this.ServiceType = serviceType;
            this.parentServices = parentServices;
            this.parentProvider = parentProvider;
        }

        public string ServiceType { get; private set; }

        public StatelessServiceBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
        {
            this.configureServiceActions.Add(configureDelegate);
            return this;
        }

        public StatelessServiceBuilder UseEndpoint(int endpoint)
        {
            if (this.listenerOptions == null)
            {
                this.listenerOptions = new ListenerOptions();
            }

            this.listenerOptions.EndPoint = endpoint;
            return this;
        }

        public StatelessService Build(StatelessServiceContext serviceContext)
        {
            IServiceCollection services = new ServiceCollection();

            // services.AddOptions();
            services.AddLogging();

            foreach (var descriptor in this.parentServices)
            {
                switch (descriptor.Lifetime)
                {
                    case ServiceLifetime.Scoped:
                    case ServiceLifetime.Transient:
                        services.Add(descriptor);
                        break;
                    case ServiceLifetime.Singleton:
                        if (!descriptor.ServiceType.ContainsGenericParameters)
                        {
                            services.AddSingleton(descriptor.ServiceType, _ => this.parentProvider.GetService(descriptor.ServiceType));
                        }

                        break;
                }
            }

            services.AddSingleton<IParentServiceProvider>(new ParentServiceProvider(this.parentProvider));
            services.AddSingleton(serviceContext);
            services.AddSingleton<ServiceContext>(serviceContext);
            services.AddSingleton(typeof(StatelessService), this.serviceClass);

            services.AddSingleton(typeof(IOptions<>), typeof(ProxyOptions<>));

            if (this.listenerOptions != null)
            {
                services.AddSingleton(typeof(ListenerOptions), this.listenerOptions);
            }

            foreach (var configureServiceAction in this.configureServiceActions)
            {
                configureServiceAction(services);
            }

            // TODO use the user specified ServiceProviderFactory to build the ServiceProvider
            this.serviceProvider = services.BuildServiceProvider();

            // return (StatelessService)Activator.CreateInstance(ProxyService.CreateProxyStatelessService(this.serviceClass, this.serviceProvider), new object[] { serviceContext, this.serviceProvider });
            return this.serviceProvider.GetService<StatelessService>();
        }
    }
}
