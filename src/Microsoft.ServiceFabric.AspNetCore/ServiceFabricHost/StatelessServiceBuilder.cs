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
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Runtime;

    public sealed class StatelessServiceBuilder
    {
        private readonly Type serviceClass;
        private readonly IServiceCollection parentServices;
        private readonly IServiceProvider parentProvider;
        private readonly Action<IHostBuilder> configureBuilder;

        internal StatelessServiceBuilder(Type serviceClass, string serviceType, IServiceCollection parentServices, IServiceProvider parentProvider, Action<IHostBuilder> configureBuilder)
        {
            this.serviceClass = serviceClass;
            this.ServiceType = serviceType;
            this.parentServices = parentServices;
            this.parentProvider = parentProvider;
            this.configureBuilder = configureBuilder;
        }

        public string ServiceType { get; private set; }

        public IHost Build(StatelessServiceContext serviceContext)
        {
#if NET461
            HostBuilder hostBuilder = new HostBuilder();
#else
            HostBuilder hostBuilder = (HostBuilder)Host.CreateDefaultBuilder();
#endif
            this.configureBuilder(hostBuilder);

            // configure global services
            hostBuilder.ConfigureServices(services =>
            {
                var included = new HashSet<Type>(services.Select(i => i.ServiceType));
                foreach (var descriptor in this.parentServices.Where(i => !included.Contains(i.ServiceType)))
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
            });

            Type serviceProxy = ServiceProxy.CreateStatelessServiceProxy(this.serviceClass);

            // Add service context and service class in DI container.
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ServiceContext>(serviceContext);
                services.AddSingleton<StatelessServiceContext>(serviceContext);
                services.AddSingleton(typeof(StatelessService), serviceProxy);
            });

            var host = hostBuilder.Build();
            return host;
        }
    }
}
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
