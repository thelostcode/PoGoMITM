using System.Threading.Tasks;
using PoGo_Proxy.Models;

namespace PoGo_Proxy.Logging
{
    public interface ILogger
    {
        Task Log(PoGoWebRequest webRequest);
    }
}
