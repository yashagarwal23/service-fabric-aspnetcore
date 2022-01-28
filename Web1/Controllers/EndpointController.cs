using System.Collections.Generic;
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

        public EndpointController(IServer server)
        {
            this.server = server;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            List<string> urls = this.server.Features.Get<IServerAddressesFeature>().Addresses.ToList();
            return urls;
        }
    }
}
