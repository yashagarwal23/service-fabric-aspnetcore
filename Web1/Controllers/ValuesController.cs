using System.Fabric;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web1.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        ServiceContext serviceContext;
        IService service;

        public ValuesController(StatelessServiceContext serviceContext, IService service)
        {
            this.serviceContext = serviceContext;
            this.service = service;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public string Get()
        {
            string result = $"Service Name: {this.serviceContext.ServiceName}, Partition: {this.serviceContext.PartitionId}, InstanceId: {this.serviceContext.ReplicaOrInstanceId}, PrintService: {this.service.Print()}";
            return result;
        }
    }
}
