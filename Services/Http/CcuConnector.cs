using System.Threading.Tasks;

namespace com.b_velop.XmlRpc.Services.Http
{
    public interface CcuConnector
    {
        Task ConnectToCcuAsync();
        Task DisconnectCcu();
    }
}