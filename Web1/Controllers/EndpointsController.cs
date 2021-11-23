using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HttpSysUrlChange.Controllers
{
    [Route("api")]
    [ApiController]
    public class EndpointsController : ControllerBase
    {
        private readonly HttpSysOptions ServerOptions;

        public EndpointsController(IOptions<HttpSysOptions> options)
        {
            ServerOptions = options.Value;
        }

        // GET: api/
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return ServerOptions.UrlPrefixes.ToArray().Select(x => x.ToString());
        }

        // GET api/add/5
        [HttpGet("add/{port}")]
        public string Add(int port)
        {
            string newPath = $"http://localhost:{port}";
            var addresses = ServerOptions.UrlPrefixes;
            addresses.Add(newPath);
            return "Path Added";
        }

        // GET api/add/5/test
        [HttpGet("add/{port}/{app}")]
        public string Add(int port, string app)
        {
            string newPath = $"http://localhost:{port}/{app}";
            var addresses = ServerOptions.UrlPrefixes;
            addresses.Add(newPath);
            return "Path Added";
        }

        // GET api/delete/5
        [HttpGet("delete/{port}")]
        public string Delete(int port)
        {
            string path = $"http://localhost:{port}";
            var addresses = ServerOptions.UrlPrefixes;
            addresses.Remove(path);
            return "Path Removed";
        }

        // GET api/delete/5/test
        [HttpGet("delete/{port}/{app}")]
        public string Delete(int port, string app)
        {
            string path = $"http://localhost:{port}/{app}";
            var addresses = ServerOptions.UrlPrefixes;
            addresses.Remove(path);
            return "Path Removed";
        }

        // GET api/long
        [HttpGet("long")]
        public string LongTask()
        {
            Thread.Sleep(10000);
            return "task done";
        }
    }
}
