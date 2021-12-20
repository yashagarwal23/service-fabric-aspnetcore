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
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Server.HttpSys;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public class StatelessService : Services.Runtime.StatelessService
    {
        private StatelessServiceContext serviceContext;
        private IServiceProvider serviceProvider;
        private string endpointName;

        public StatelessService(StatelessServiceContext serviceContext, string endpointName, IServiceProvider serviceProvider)
            : base(serviceContext)
        {
            this.serviceContext = serviceContext;
            this.serviceProvider = serviceProvider;
            this.endpointName = endpointName;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
                {
                    new ServiceInstanceListener(serviceContext =>
                        new DefaultWebHostCommunicationListener(serviceContext, this.GetListenerUrl(), this.serviceProvider)),
                };
        }

        private string GetListenerUrl()
        {
            var serviceEndpoint = this.GetEndpointResourceDescription(this.endpointName);
            var listenUrl = string.Format(
                CultureInfo.InvariantCulture,
                "{0}://+:{1}",
                serviceEndpoint.Protocol.ToString().ToLowerInvariant(),
                serviceEndpoint.Port);

            this.serviceProvider.GetService<IReplicaResolutionStrategy>().MapPort(serviceEndpoint.Port.ToString(), this.serviceContext.PartitionId.ToString() + this.serviceContext.InstanceId.ToString());

            return listenUrl;
        }

        private EndpointResourceDescription GetEndpointResourceDescription(string endpointName)
        {
            if (endpointName == null)
            {
                throw new ArgumentNullException("endpointName");
            }

            if (!this.serviceContext.CodePackageActivationContext.GetEndpoints().Contains(endpointName))
            {
                throw new InvalidOperationException(string.Format(SR.EndpointNameNotFoundExceptionMessage, endpointName));
            }

            return this.serviceContext.CodePackageActivationContext.GetEndpoint(endpointName);
        }
    }

    internal class DefaultWebHostCommunicationListener : ICommunicationListener
    {
        private ServiceContext serviceContext;
        private string listenUrl;
        private IServiceProvider serviceProvider;

        public DefaultWebHostCommunicationListener(ServiceContext serviceContext, string listenUrl, IServiceProvider serviceProvider)
        {
            this.serviceContext = serviceContext;
            this.listenUrl = listenUrl;

            // this.listenUrl = listenUrl.Replace("://+:", $"://localhost:");
            this.serviceProvider = serviceProvider;
        }

        public void Abort()
        {
            var serverOptions = this.serviceProvider.GetRequiredService<IOptions<HttpSysOptions>>().Value;
            var addresses = serverOptions.UrlPrefixes;
            addresses.Remove(this.listenUrl);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            var serverOptions = this.serviceProvider.GetRequiredService<IOptions<HttpSysOptions>>().Value;
            var addresses = serverOptions.UrlPrefixes;
            addresses.Remove(this.listenUrl);
            return Task.CompletedTask;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serverOptions = this.serviceProvider.GetRequiredService<IOptions<HttpSysOptions>>().Value;
            var addresses = serverOptions.UrlPrefixes;
            addresses.Add(this.listenUrl);

            var publishAddress = this.serviceContext.PublishAddress;
            return Task.FromResult(this.listenUrl.Replace("://+:", $"://{publishAddress}:"));
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore SA1600 // Elements should be documented
