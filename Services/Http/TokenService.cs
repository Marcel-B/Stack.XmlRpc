using System.Threading.Tasks;
using com.b_velop.XmlRpc.Models;

namespace com.b_velop.XmlRpc.Services.Http
{
    public interface TokenService
    {

        Task<Token> RequestTokenAsync();
    }
}
