using System;
using System.Fabric;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web1.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public ServiceController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        // GET: api/<ServiceController>
        [HttpGet]
        public string Get()
        {
            Thread.Sleep(10000);
            var serviceContext = this.serviceProvider.GetRequiredService<StatelessServiceContext>();
            string result = $"Service Name: {serviceContext.ServiceName}, Partition: {serviceContext.PartitionId}, InstanceId: {serviceContext.InstanceId}";
            return result;
        }
    }
}
