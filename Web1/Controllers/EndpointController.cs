using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web1.Controllers
{
    [Route("api/")]
    [ApiController]
    public class EndpointController : ControllerBase
    {
        private HttpSysOptions options; 

        public EndpointController(IOptions<HttpSysOptions> options)
        {
            this.options = options.Value;
        }

        [HttpGet("end/")]
        public IEnumerable<string> Get()
        {
            var addresses = this.options.UrlPrefixes.ToArray();
            return addresses.Select(i => i.ToString());
        }
    }
}
