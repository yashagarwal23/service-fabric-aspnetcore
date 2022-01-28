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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class ServiceBuilder : IHostBuilder
    {
#pragma warning disable SA1401 // Fields should be private
        protected IHostBuilder hostBuilder;
#pragma warning restore SA1401 // Fields should be private

        internal ServiceBuilder()
        {
#if NET461
            this.hostBuilder = new HostBuilder();
#else
            this.hostBuilder = Host.CreateDefaultBuilder();
#endif
        }

        public IDictionary<object, object> Properties { get => this.hostBuilder.Properties; }

        public IHost Build()
        {
            throw new InvalidOperationException("Building StatelessServiceBuilder/StatefulServiceBuilder not allowed");
        }

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            this.hostBuilder.ConfigureAppConfiguration(configureDelegate);
            return this;
        }

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            this.hostBuilder.ConfigureContainer<TContainerBuilder>(configureDelegate);
            return this;
        }

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            this.hostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            this.hostBuilder.ConfigureServices(configureDelegate);
            return this;
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            this.hostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this;
        }

#if !NET461
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            this.hostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this;
        }
#endif
    }
}

#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
