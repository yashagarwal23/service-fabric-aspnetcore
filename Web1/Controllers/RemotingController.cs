using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;


namespace Web1.Controllers
{
    [Route("remoting")]
    [ApiController]
    public class RemotingController : ControllerBase
    {
        private ServiceContext serviceContext;

        public RemotingController(ServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            if (this.serviceContext.ServiceTypeName == "Web1Type")
            {
                return "not allowed";
            }

            int num = new Random().Next();
            var webServiceProxy = ServiceProxy.Create<ServiceRemotingInt>(new Uri("fabric:/TestApp/Web1"));
            await webServiceProxy.SetNum(num);
            return $"num: {num} set on Web1Type service";
        }
    }
}
