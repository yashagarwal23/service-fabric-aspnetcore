using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2, RemotingClientVersion = RemotingClientVersion.V2)]
namespace Web1
{
    public interface ServiceRemotingInt : IService
    {
        Task SetNum(int num);
    }

    //public class ServiceRemotingImpl : ServiceRemotingInt
    //{
    //    private PrintService printService;

    //    public ServiceRemotingImpl(PrintService printService)
    //    {
    //        this.printService = printService;
    //    }

    //    public Task SetNum(int num)
    //    {
    //        this.printService.SetNum(num);
    //        return Task.CompletedTask;
    //    }
    //}
}
