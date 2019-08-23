using System.IO;
using System.Threading.Tasks;

namespace com.b_velop.XmlRpc.BL
{
    public interface Parser
    {
        Task Parse(Stream inputStream);
    }
}