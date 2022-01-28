namespace Web1
{
    using System.Fabric;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class Web1 : AspNetStatelessService, ServiceRemotingInt
    {
        private PrintService printService;

        public Web1(StatelessServiceContext context, PrintService printService)
            : base(context)
        {
            this.printService = printService;
        }

        public Task SetNum(int num)
        {
            this.printService.SetNum(num);
            return Task.CompletedTask;
        }
    }
}
