using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;

namespace Web1
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    class Web1 : StatelessService
    {
        private IServiceProvider serviceProvider;

        public Web1(StatelessServiceContext context, IServiceProvider serviceProvider)
            : base(context)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            var serviceContext = this.serviceProvider.GetService<ServiceContext>();
            var printService = this.serviceProvider.GetService<IService>();
            return Task.CompletedTask;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[] {
                new ServiceInstanceListener(context => new CommonWebHostCommunicationListener(this.serviceProvider)),
            };
        }
    }
}
