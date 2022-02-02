using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web1.Controllers
{
    [Route("endpoint")]
    [ApiController]
    public class EndpointController : ControllerBase
    {
        private IServer server;
        private IServicePartition partition;

        public EndpointController(IServer server, IServicePartition partition)
        {
            this.server = server;
            this.partition = partition;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            List<string> urls = this.server.Features.Get<IServerAddressesFeature>().Addresses.ToList();
            return urls;
        }

        [HttpGet("partition")]
        public ServicePartitionInformation GetPartition()
        {
            return this.partition.PartitionInfo;
        }
    }
}
